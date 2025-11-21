using BE.API.DTOs.Request;
using BE.API.DTOs.Response;
using BE.BOs.Models;
using BE.REPOs.Interface;
using Microsoft.EntityFrameworkCore;
using BE.REPOs.Service;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepo _userRepo;
        private readonly IConfiguration _configuration;
        private readonly CloudinaryService _cloudinary;
        private readonly IEmailService _emailService;
        private readonly IOTPService _otpService;

        public UserController(IUserRepo userRepo, IConfiguration configuration, CloudinaryService cloudinaryService, IEmailService emailService, IOTPService otpService)
        {
            _userRepo = userRepo;
            _configuration = configuration;
            _cloudinary = cloudinaryService;
            _emailService = emailService;
            _otpService = otpService;
        }

        // ========== Admin: helpers ==========
        private bool IsAdminOrSubAdminFromClaims()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
            if (string.IsNullOrEmpty(roleClaim)) return false;
            // Accept numeric RoleId in token (1: Admin, 3: SubAdmin) or textual
            if (int.TryParse(roleClaim, out var roleId))
            {
                return roleId == 1 || roleId == 3;
            }
            return roleClaim.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                || roleClaim.Equals("SubAdmin", StringComparison.OrdinalIgnoreCase)
                || roleClaim.Equals("Moderator", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeRoleNameToUi(string? roleName)
        {
            if (string.IsNullOrEmpty(roleName)) return "user";
            if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase)) return "admin";
            if (roleName.Equals("Staff", StringComparison.OrdinalIgnoreCase)) return "staff";  
            return "user";
        }


        private static string MapUiRoleToRoleName(string uiRole)
        {
            if (uiRole.Equals("admin", StringComparison.OrdinalIgnoreCase)) return "Admin";
            if (uiRole.Equals("sub_admin", StringComparison.OrdinalIgnoreCase)) return "SubAdmin";
            return "Member";
        }

        private static string MapUiStatusToDb(string uiStatus)
        {
            if (uiStatus.Equals("active", StringComparison.OrdinalIgnoreCase)) return "Active";
            if (uiStatus.Equals("suspended", StringComparison.OrdinalIgnoreCase)) return "Suspended";
            if (uiStatus.Equals("deleted", StringComparison.OrdinalIgnoreCase)) return "Deleted";
            return uiStatus;
        }

        private static string NormalizeDbStatusToUi(string? dbStatus)
        {
            if (string.IsNullOrEmpty(dbStatus)) return "active";
            if (dbStatus.Equals("Active", StringComparison.OrdinalIgnoreCase)) return "active";
            if (dbStatus.Equals("Suspended", StringComparison.OrdinalIgnoreCase)) return "suspended";
            if (dbStatus.Equals("Deleted", StringComparison.OrdinalIgnoreCase)) return "deleted";
            return dbStatus.ToLower();
        }

        private static bool IsAccountActive(User? user)
        {
            if (user == null) return false;
            
            // Lấy AccountStatus trực tiếp, không normalize trước
            var accountStatus = user.AccountStatus;
            
            // Nếu AccountStatus là null hoặc empty, coi như active (backward compatibility)
            // NHƯNG nếu đã được set rõ ràng là "Deleted" hoặc "Suspended" thì không cho phép
            if (string.IsNullOrWhiteSpace(accountStatus))
            {
                return true; // Mặc định là active nếu chưa set
            }
            
            // Kiểm tra trực tiếp với case-insensitive
            var statusUpper = accountStatus.Trim().ToUpperInvariant();
            
            // Chỉ cho phép login nếu status là "ACTIVE"
            // Tất cả các trạng thái khác (SUSPENDED, DELETED, hoặc bất kỳ giá trị nào) đều không cho phép login
            return statusUpper == "ACTIVE";
        }

        [HttpPost("login")]
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
        {
            // GetAccountByEmailAndPassword đã verify password rồi, nếu trả về null thì email/password sai
            var user = _userRepo.GetAccountByEmailAndPassword(request.Email, request.Password);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid email or password.");
            }

            // QUAN TRỌNG: Reload user từ database để đảm bảo có AccountStatus mới nhất
            // Vì có thể user object đã bị cached hoặc không được refresh
            using var context = new EvandBatteryTradingPlatformContext();
            var freshUser = context.Users.FirstOrDefault(u => u.UserId == user.UserId);
            if (freshUser == null)
            {
                return Unauthorized("User not found.");
            }

            // Kiểm tra trạng thái tài khoản TRƯỚC KHI tạo token
            // Đây là bước quan trọng để ngăn user bị deleted/suspended đăng nhập
            if (!IsAccountActive(freshUser))
            {
                var status = NormalizeDbStatusToUi(freshUser.AccountStatus);
                string message = status.Equals("deleted", StringComparison.OrdinalIgnoreCase)
                    ? "Tài khoản đã bị xóa và không thể đăng nhập."
                    : status.Equals("suspended", StringComparison.OrdinalIgnoreCase)
                    ? "Tài khoản đã bị khóa và không thể đăng nhập."
                    : "Tài khoản không thể đăng nhập do trạng thái không hợp lệ.";
                
                return Unauthorized(new { message = message, status = status });
            }

            // Chỉ tạo token khi tài khoản đang active
            var token = GenerateJwtToken(freshUser);
            return Ok(new LoginResponse
            {
                Role = freshUser.RoleId?.ToString() ?? "Member",
                Token = token,
                AccountId = freshUser.UserId.ToString()
            });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.FullName ?? "Unknown"),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserId", user.UserId.ToString())
            };

            if (user.RoleId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.Role, user.RoleId.Value.ToString()));
            }

            var secretKey = _configuration["JWT:SecretKey"] ?? "default-secret-key";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 🔥 Token sống 100 năm (cơ bản là “vĩnh viễn”)
            var preparedToken = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddYears(100),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(preparedToken);
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register([FromForm] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest("Email and password are required");
                }

                // Validate email format
                if (!request.Email.Contains("@") || !request.Email.Contains("."))
                {
                    return BadRequest("Email không hợp lệ");
                }

                // Validate password length
                if (request.Password.Length < 6)
                {
                    return BadRequest("Mật khẩu phải có ít nhất 6 ký tự");
                }

                // ✅ Upload avatar nếu có
                string? avatarUrl = null;
                if (request.Avatar != null)
                {
                    try
                    {
                        avatarUrl = await _cloudinary.UploadImageAsync(request.Avatar);
                    }
                    catch (Exception uploadEx)
                    {
                        // Log lỗi upload nhưng vẫn tiếp tục đăng ký
                        Console.WriteLine($"Lỗi upload avatar: {uploadEx.Message}");
                    }
                }

                // ✅ Tạo user mới
                var user = new User
                {
                    Email = request.Email.Trim().ToLower(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    FullName = request.FullName?.Trim(),
                    Phone = request.Phone?.Trim(),
                    Avatar = avatarUrl,
                    RoleId = 2 // member mặc định
                };

                var registeredUser = _userRepo.Register(user);

                return Ok(new
                {
                    registeredUser.UserId,
                    registeredUser.Email,
                    registeredUser.FullName,
                    registeredUser.Phone,
                    registeredUser.Avatar,
                    registeredUser.RoleId
                });
            }
            catch (Exception ex)
            {
                // Trả về message lỗi cụ thể hơn
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " | Inner: " + ex.InnerException.Message;
                }
                return StatusCode(500, "Internal server error: " + errorMessage);
            }
        }

        // ========== Admin: Users List ==========
        [HttpGet]
        [Route("/api/admin/users")] // absolute route
        [Authorize]
        public ActionResult<PagedResponse<AdminUserListItemResponse>> AdminGetUsers([FromQuery] AdminUserListQuery query)
        {
            if (!IsAdminOrSubAdminFromClaims()) return Forbid();

            using var context = new EvandBatteryTradingPlatformContext();
            var users = context.Users.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLower();
                users = users.Where(u => (u.Email != null && u.Email.ToLower().Contains(s))
                                         || (u.FullName != null && u.FullName.ToLower().Contains(s))
                                         || (u.Phone != null && u.Phone.ToLower().Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(query.Role))
            {
                var roleName = MapUiRoleToRoleName(query.Role);
                users = users.Where(u => u.Role != null && u.Role.RoleName == roleName);
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var status = MapUiStatusToDb(query.Status);
                users = users.Where(u => u.AccountStatus != null && u.AccountStatus == status);
            }

            var sort = string.IsNullOrWhiteSpace(query.Sort) ? "createdAt:desc" : query.Sort;
            foreach (var part in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var tokens = part.Split(':', StringSplitOptions.TrimEntries);
                var field = tokens[0].ToLower();
                var dir = tokens.Length > 1 ? tokens[1].ToLower() : "asc";
                bool desc = dir == "desc";

                users = (field) switch
                {
                    "createdat" => desc ? users.OrderByDescending(u => u.CreatedDate) : users.OrderBy(u => u.CreatedDate),
                    "fullname" => desc ? users.OrderByDescending(u => u.FullName) : users.OrderBy(u => u.FullName),
                    "email" => desc ? users.OrderByDescending(u => u.Email) : users.OrderBy(u => u.Email),
                    _ => desc ? users.OrderByDescending(u => u.UserId) : users.OrderBy(u => u.UserId)
                };
            }

            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize < 1 ? 20 : (query.PageSize > 200 ? 200 : query.PageSize);
            var totalItems = users.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var userList = users.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(u => new
                {
                    u.UserId,
                    u.FullName,
                    u.Email,
                    u.AccountStatus,
                    u.AccountStatusReason,
                    u.StatusChangedDate,
                    u.CreatedDate,
                    RoleName = u.Role != null ? u.Role.RoleName : null
                })
                .ToList();
            
            var items = userList.Select(u => new AdminUserListItemResponse
                {
                    Id = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = NormalizeRoleNameToUi(u.RoleName),
                    Status = NormalizeDbStatusToUi(u.AccountStatus),
                    AccountStatusReason = u.AccountStatusReason,
                    Reason = u.AccountStatusReason ?? string.Empty,
                    StatusChangedDate = u.StatusChangedDate,
                    CreatedAt = u.CreatedDate,
                    LastLoginAt = null
                })
                .ToList();

            return Ok(new PagedResponse<AdminUserListItemResponse>
            {
                Items = items,
                Meta = new PageMeta { Page = page, PageSize = pageSize, TotalItems = totalItems, TotalPages = totalPages, Sort = sort.Split(',').ToList() }
            });
        }

        // ========== Admin: User Detail ==========
        [HttpGet]
        [Route("/api/admin/users/{id}")]
        [Authorize]
        public ActionResult<AdminUserDetailResponse> AdminGetUserDetail([FromRoute] int id)
        {
            if (!IsAdminOrSubAdminFromClaims()) return Forbid();

            using var context = new EvandBatteryTradingPlatformContext();
            var user = context.Users.Include(u => u.Role).FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound();

            var orderCount = context.Orders.Count(o => o.BuyerId == id || o.SellerId == id);
            var listingCount = context.Products.Count(p => p.SellerId == id);
            var violationCount = context.ReportedListings.Include(r => r.Product).Count(r => r.Product != null && r.Product.SellerId == id);

            return Ok(new AdminUserDetailResponse
            {
                Id = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Avatar = user.Avatar,
                Role = NormalizeRoleNameToUi(user.Role != null ? user.Role.RoleName : null),
                Status = NormalizeDbStatusToUi(user.AccountStatus),
                AccountStatusReason = user.AccountStatusReason,
                StatusChangedDate = user.StatusChangedDate,
                CreatedAt = user.CreatedDate,
                LastLoginAt = null,
                Stats = new AdminUserStats { OrderCount = orderCount, ListingCount = listingCount, ViolationCount = violationCount }
            });
        }

        // ========== Admin: Update basic ==========
        [HttpPut]
        [Route("/api/admin/users/{id}")]
        [Authorize]
        public ActionResult<AdminUserDetailResponse> AdminUpdateBasic([FromRoute] int id, [FromBody] AdminUserUpdateRequest request)
        {
            if (!IsAdminOrSubAdminFromClaims()) return Forbid();

            var user = _userRepo.GetUserById(id);
            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(request.Email)) user.Email = request.Email;
            user.FullName = request.FullName;
            user.Phone = request.Phone;
            user.Avatar = request.Avatar;

            var updated = _userRepo.UpdateUser(user);
            updated = _userRepo.GetUserById(updated.UserId);

            return Ok(new AdminUserDetailResponse
            {
                Id = updated.UserId,
                FullName = updated.FullName,
                Email = updated.Email,
                Phone = updated.Phone,
                Avatar = updated.Avatar,
                Role = NormalizeRoleNameToUi(updated.Role != null ? updated.Role.RoleName : null),
                Status = NormalizeDbStatusToUi(updated.AccountStatus),
                AccountStatusReason = updated.AccountStatusReason,
                StatusChangedDate = updated.StatusChangedDate,
                CreatedAt = updated.CreatedDate,
                LastLoginAt = null,
                Stats = new AdminUserStats { OrderCount = 0, ListingCount = 0, ViolationCount = 0 }
            });
        }

        // ========== Admin: Update role ==========
        [HttpPut]
        [Route("/api/admin/users/{id}/role")]
        [Authorize]
        public ActionResult<AdminUserDetailResponse> AdminUpdateRole([FromRoute] int id, [FromBody] AdminUserRoleRequest request)
        {
            if (!IsAdminOrSubAdminFromClaims()) return Forbid();

            using var context = new EvandBatteryTradingPlatformContext();
            var user = context.Users.Include(u => u.Role).FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound();

            var targetRoleName = MapUiRoleToRoleName(request.Role);
            var role = context.UserRoles.FirstOrDefault(r => r.RoleName == targetRoleName);
            if (role == null)
            {
                return UnprocessableEntity(new { error = new { code = "INVALID_ROLE", message = "Role does not exist" } });
            }

            var isCurrentAdmin = user.Role != null && user.Role.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            var isDowngradeFromAdmin = isCurrentAdmin && !targetRoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            if (isDowngradeFromAdmin)
            {
                var adminCount = context.Users.Include(u => u.Role).Count(u => u.Role != null && u.Role.RoleName == "Admin");
                if (adminCount <= 1)
                {
                    return Conflict(new { error = new { code = "LAST_ADMIN_DOWNGRADE_FORBIDDEN", message = "Cannot downgrade the last admin" } });
                }
            }

            user.RoleId = role.RoleId;
            context.Users.Update(user);
            context.SaveChanges();

            user = context.Users.Include(u => u.Role).First(u => u.UserId == id);
            return Ok(new AdminUserDetailResponse
            {
                Id = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Avatar = user.Avatar,
                Role = NormalizeRoleNameToUi(user.Role != null ? user.Role.RoleName : null),
                Status = NormalizeDbStatusToUi(user.AccountStatus),
                AccountStatusReason = user.AccountStatusReason,
                StatusChangedDate = user.StatusChangedDate,
                CreatedAt = user.CreatedDate,
                LastLoginAt = null,
                Stats = new AdminUserStats { OrderCount = 0, ListingCount = 0, ViolationCount = 0 }
            });
        }

        // ========== Admin: Update status ==========
        [HttpPut]
        [Route("/api/admin/users/{id}/status")]
        [Authorize]
        public ActionResult<AdminUserDetailResponse> AdminUpdateStatus([FromRoute] int id, [FromBody] AdminUserStatusRequest request)
        {
            if (!IsAdminOrSubAdminFromClaims()) return Forbid();

            using var context = new EvandBatteryTradingPlatformContext();
            var user = context.Users.Include(u => u.Role).FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound();

            var newStatus = MapUiStatusToDb(request.Status);
            user.AccountStatus = newStatus;
            user.AccountStatusReason = request.Reason;
            user.StatusChangedDate = DateTime.Now;
            context.Users.Update(user);
            context.SaveChanges();

            user = context.Users.Include(u => u.Role).First(u => u.UserId == id);
            return Ok(new AdminUserDetailResponse
            {
                Id = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Avatar = user.Avatar,
                Role = NormalizeRoleNameToUi(user.Role != null ? user.Role.RoleName : null),
                Status = NormalizeDbStatusToUi(user.AccountStatus),
                AccountStatusReason = user.AccountStatusReason,
                StatusChangedDate = user.StatusChangedDate,
                CreatedAt = user.CreatedDate,
                LastLoginAt = null,
                Stats = new AdminUserStats { OrderCount = 0, ListingCount = 0, ViolationCount = 0 }
              
            })
            ;
        }

        // ========== Admin: Check status ==========
        [HttpGet]
        [Route("/api/admin/users/{id}/status")]
        [Authorize]
        public ActionResult<AdminUserStatusResponse> AdminGetUserStatus([FromRoute] int id)
        {
            if (!IsAdminOrSubAdminFromClaims()) return Forbid();

            using var context = new EvandBatteryTradingPlatformContext();
            var user = context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound(new { message = "User not found" });

            var normalizedStatus = NormalizeDbStatusToUi(user.AccountStatus);
            var canLogin = normalizedStatus.Equals("active", StringComparison.OrdinalIgnoreCase);
            
            string message;
            if (canLogin)
            {
                message = "Tài khoản đang hoạt động bình thường và có thể đăng nhập.";
            }
            else if (normalizedStatus.Equals("suspended", StringComparison.OrdinalIgnoreCase))
            {
                message = "Tài khoản đã bị khóa (suspended) và không thể đăng nhập.";
            }
            else if (normalizedStatus.Equals("deleted", StringComparison.OrdinalIgnoreCase))
            {
                message = "Tài khoản đã bị xóa (deleted) và không thể đăng nhập.";
            }
            else
            {
                message = $"Tài khoản có trạng thái '{normalizedStatus}' và không thể đăng nhập.";
            }

            return Ok(new AdminUserStatusResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                Status = normalizedStatus,
                CanLogin = canLogin,
                AccountStatusReason = user.AccountStatusReason,
                Message = message
            });
        }

        // ========== Admin: Audit ==========
        [HttpGet]
        [Route("/api/admin/users/{id}/audit")]
        [Authorize]
        public ActionResult<PagedResponse<AdminAuditItemResponse>> AdminGetAudit([FromRoute] int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (!IsAdminOrSubAdminFromClaims()) return Forbid();

            var safePage = page < 1 ? 1 : page;
            var safePageSize = pageSize < 1 ? 20 : (pageSize > 200 ? 200 : pageSize);
            return Ok(new PagedResponse<AdminAuditItemResponse>
            {
                Items = new List<AdminAuditItemResponse>(),
                Meta = new PageMeta { Page = safePage, PageSize = safePageSize, TotalItems = 0, TotalPages = 0, Sort = new List<string>() }
            });
        }

        [HttpGet]
        public ActionResult<IEnumerable<UserResponse>> GetAllUsers()
        {
            try
            {
                var users = _userRepo.GetAllUsers();
                var response = users.Select(user => new UserResponse
                {
                    UserId = user.UserId,
                    Role = user.Role?.RoleName ?? "Unknown",
                    Email = user.Email,
                    FullName = user.FullName,
                    Phone = user.Phone,
                    Avatar = user.Avatar,
                    AccountStatus = user.AccountStatus,
                    CreatedDate = user.CreatedDate
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("{id}")]
        public ActionResult<User> GetUser(int id)
        {
            try
            {
                var user = _userRepo.GetUserById(id);
                if (user == null)
                {
                    return NotFound();
                }
                var response = new UserResponse
                {
                    UserId = user.UserId,
                    Role = user.Role?.RoleName ?? "Unknown",
                    Email = user.Email,
                    FullName = user.FullName,
                    Phone = user.Phone,
                    Avatar = user.Avatar,
                    AccountStatus = user.AccountStatus,
                    CreatedDate = user.CreatedDate
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        public ActionResult<UserResponse> UpdateUser(int id, [FromBody] UserRequest request)
        {
            try
            {
                var existingUser = _userRepo.GetUserById(id);
                if (existingUser == null)
                {
                    return NotFound();
                }

                existingUser.FullName = request.FullName;
                existingUser.Phone = request.Phone;
                existingUser.Avatar = request.Avatar;

                var updatedUser = _userRepo.UpdateUser(existingUser);

                var response = new
                {
                    FullName = updatedUser.FullName,
                    Phone = updatedUser.Phone,
                    Avatar = updatedUser.Avatar
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteUser(int id)
        {
            try
            {
                var result = _userRepo.DeleteUser(id);
                if (!result)
                {
                    return NotFound();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                // Validate email
                if (string.IsNullOrEmpty(request.Email))
                {
                    return BadRequest(new ForgotPasswordResponse
                    {
                        Success = false,
                        Message = "Email is required"
                    });
                }

                // Check if user exists
                var user = _userRepo.GetUserByEmail(request.Email);
                if (user == null)
                {
                    // For security, don't reveal if email exists or not
                    return Ok(new ForgotPasswordResponse
                    {
                        Success = true,
                        Message = "If the email exists, a password reset link has been sent"
                    });
                }

                // Generate OTP
                var otp = _otpService.GenerateOTP();
                var otpExpiry = DateTime.Now.AddMinutes(10); // OTP expires in 10 minutes

                // Update user with OTP
                var success = _userRepo.UpdateResetPasswordToken(request.Email, otp, otpExpiry);
                if (!success)
                {
                    return StatusCode(500, new ForgotPasswordResponse
                    {
                        Success = false,
                        Message = "Failed to generate OTP"
                    });
                }

                // Send email with OTP
                try
                {
                    // Send real email
                    await _emailService.SendPasswordResetEmailAsync(request.Email, otp);
                    
                    // Debug info for development
                    Console.WriteLine($"📧 Real Email sent to: {request.Email}");
                    Console.WriteLine($"🔑 OTP: {otp}");
                    Console.WriteLine($"⏰ OTP expires in 10 minutes");
                }
                catch (Exception emailEx)
                {
                    // Log email error and return error response for development
                    Console.WriteLine($"Failed to send email: {emailEx.Message}");
                    Console.WriteLine($"Full exception: {emailEx}");
                    return StatusCode(500, new ForgotPasswordResponse
                    {
                        Success = false,
                        Message = $"Failed to send email: {emailEx.Message}"
                    });
                }

                // For development mode, also return token in response
                var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
                
                return Ok(new ForgotPasswordResponse
                {
                    Success = true,
                    Message = "OTP has been sent to your email",
                    ResetToken = isDevelopment ? otp : null // Only show OTP in development
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ForgotPasswordResponse
                {
                    Success = false,
                    Message = "Internal server error: " + ex.Message
                });
            }
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public ActionResult<ResetPasswordResponse> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(request.Token))
                {
                    return BadRequest(new ResetPasswordResponse
                    {
                        Success = false,
                        Message = "OTP is required"
                    });
                }

                // Validate OTP format (6 digits)
                if (!_otpService.ValidateOTP(request.Token))
                {
                    return BadRequest(new ResetPasswordResponse
                    {
                        Success = false,
                        Message = "Invalid OTP format. OTP must be 6 digits."
                    });
                }

                if (string.IsNullOrEmpty(request.NewPassword))
                {
                    return BadRequest(new ResetPasswordResponse
                    {
                        Success = false,
                        Message = "New password is required"
                    });
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    return BadRequest(new ResetPasswordResponse
                    {
                        Success = false,
                        Message = "Passwords do not match"
                    });
                }

                if (request.NewPassword.Length < 6)
                {
                    return BadRequest(new ResetPasswordResponse
                    {
                        Success = false,
                        Message = "Password must be at least 6 characters long"
                    });
                }

                // Verify OTP and reset password
                var success = _userRepo.ResetPassword(request.Token, request.NewPassword);
                if (!success)
                {
                    return BadRequest(new ResetPasswordResponse
                    {
                        Success = false,
                        Message = "Invalid or expired OTP"
                    });
                }

                return Ok(new ResetPasswordResponse
                {
                    Success = true,
                    Message = "Password has been reset successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResetPasswordResponse
                {
                    Success = false,
                    Message = "Internal server error: " + ex.Message
                });
            }
        }



        [HttpGet("test-jwt")]
        [Authorize]
        public ActionResult TestJWT()
        {
            var userId = User.FindFirst("UserId")?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                message = "JWT token is valid!",
                userId = userId,
                email = email,
                name = name,
                role = role,
                timestamp = DateTime.Now
            });
        }

        [HttpPost("test-admin-login")]
        [AllowAnonymous]
        public ActionResult TestAdminLogin()
        {
            try
            {
                var adminEmail = "admin@evtrading.com";
                var adminPassword = "admin123";
                
                // Tìm admin trong database
                var admin = _userRepo.GetUserByEmail(adminEmail);
                if (admin == null)
                {
                    return NotFound(new
                    {
                        message = "Admin không tồn tại trong database",
                        suggestion = "Chạy script CreateAdminFixed.sql hoặc gọi API create-initial-admin"
                    });
                }

                // Test password verification
                var passwordValid = BCrypt.Net.BCrypt.Verify(adminPassword, admin.PasswordHash);
                
                return Ok(new
                {
                    message = "Admin login test",
                    admin = new
                    {
                        userId = admin.UserId,
                        email = admin.Email,
                        fullName = admin.FullName,
                        roleId = admin.RoleId,
                        accountStatus = admin.AccountStatus
                    },
                    passwordTest = new
                    {
                        plainPassword = adminPassword,
                        hashedPassword = admin.PasswordHash,
                        passwordValid = passwordValid,
                        hashLength = admin.PasswordHash?.Length
                    },
                    recommendation = passwordValid ? 
                        "Password verification OK - có thể đăng nhập" : 
                        "Password verification FAILED - cần tạo lại admin"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi test admin login: " + ex.Message });
            }
        }

        [HttpPost("create-initial-admin")]
        [AllowAnonymous]
        public ActionResult CreateInitialAdmin()
        {
            try
            {
                // Kiểm tra xem đã có admin nào chưa
                var existingAdmins = _userRepo.GetAllUsers().Where(u => u.RoleId == 1).ToList();
                if (existingAdmins.Any())
                {
                    return BadRequest(new
                    {
                        message = "Admin đã tồn tại trong hệ thống!",
                        existingAdmins = existingAdmins.Select(a => new { a.UserId, a.Email, a.FullName }).ToList()
                    });
                }

                // Tạo admin đầu tiên với password đã hash
                var plainPassword = "admin123";
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);
                
                var adminUser = new User
                {
                    Email = "admin@evtrading.com",
                    PasswordHash = hashedPassword,
                    FullName = "System Administrator",
                    Phone = "0123456789",
                    RoleId = 1, // Admin role
                    AccountStatus = "Active",
                    CreatedDate = DateTime.Now
                };

                var createdAdmin = _userRepo.Register(adminUser);

                // Debug: In ra thông tin để kiểm tra
                Console.WriteLine($"[DEBUG] Admin created:");
                Console.WriteLine($"  Email: {createdAdmin.Email}");
                Console.WriteLine($"  PasswordHash: {createdAdmin.PasswordHash}");
                Console.WriteLine($"  RoleId: {createdAdmin.RoleId}");

                return Ok(new
                {
                    message = "Admin đầu tiên đã được tạo thành công!",
                    admin = new
                    {
                        userId = createdAdmin.UserId,
                        email = createdAdmin.Email,
                        fullName = createdAdmin.FullName,
                        roleId = createdAdmin.RoleId,
                        createdDate = createdAdmin.CreatedDate
                    },
                    loginInfo = new
                    {
                        email = "admin@evtrading.com",
                        password = "admin123",
                        note = "Vui lòng đổi password sau khi đăng nhập lần đầu!"
                    },
                    debug = new
                    {
                        hashedPassword = hashedPassword,
                        verificationTest = BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi tạo admin: " + ex.Message });
            }
        }
    }
}

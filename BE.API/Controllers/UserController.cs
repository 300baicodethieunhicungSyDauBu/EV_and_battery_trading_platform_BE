using BE.API.DTOs.Request;
using BE.API.DTOs.Response;
using BE.BOs.Models;
using BE.REPOs.Interface;
using BE.REPOs.Service;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
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

        [HttpPost("login")]
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
        {
            var user = _userRepo.GetAccountByEmailAndPassword(request.Email, request.Password);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid email or password.");
            }


            // Generate JWT Token
            var token = GenerateJwtToken(user);
            return Ok(new LoginResponse
            {
                Role = user.RoleId?.ToString() ?? "Member",
                Token = token,
                AccountId = user.UserId.ToString()
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

                // ✅ Upload avatar nếu có
                string? avatarUrl = null;
                if (request.Avatar != null)
                {
                    avatarUrl = await _cloudinary.UploadImageAsync(request.Avatar);
                }

                // ✅ Tạo user mới
                var user = new User
                {
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    FullName = request.FullName,
                    Phone = request.Phone,
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
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
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

        // OAuth Login Endpoints
        [HttpGet("google-login")]
        [AllowAnonymous]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/api/User/google-callback"
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("facebook-login")]
        [AllowAnonymous]
        public IActionResult FacebookLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/api/User/facebook-callback"
            };
            return Challenge(properties, FacebookDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        [AllowAnonymous]
        public IActionResult GoogleCallback()
        {
            var result = HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme).Result;
            if (!result.Succeeded)
            {
                return BadRequest("Google authentication failed");
            }

            var claims = result.Principal.Claims;
            var email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            var googleId = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var avatar = claims.FirstOrDefault(x => x.Type == "picture")?.Value;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleId))
            {
                return BadRequest("Unable to retrieve user information from Google");
            }

            return ProcessOAuthLogin("Google", googleId, email, name ?? "Google User", avatar).Result;
        }

        [HttpGet("facebook-callback")]
        [AllowAnonymous]
        public IActionResult FacebookCallback()
        {
            var result = HttpContext.AuthenticateAsync(FacebookDefaults.AuthenticationScheme).Result;
            if (!result.Succeeded)
            {
                return BadRequest("Facebook authentication failed");
            }

            var claims = result.Principal.Claims;
            var email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            var facebookId = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var avatar = claims.FirstOrDefault(x => x.Type == "picture")?.Value;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(facebookId))
            {
                return BadRequest("Unable to retrieve user information from Facebook");
            }

            return ProcessOAuthLogin("Facebook", facebookId, email, name ?? "Facebook User", avatar).Result;
        }

        private Task<IActionResult> ProcessOAuthLogin(string provider, string oauthId, string email, string fullName, string? avatar)
        {
            try
            {
                // Check if user already exists with this OAuth account
                var existingOAuthUser = _userRepo.GetUserByOAuth(provider, oauthId);
                if (existingOAuthUser != null)
                {
                    var token = GenerateJwtToken(existingOAuthUser);
                    return Task.FromResult<IActionResult>(Ok(new LoginResponse
                    {
                        Role = existingOAuthUser.RoleId?.ToString() ?? "Member",
                        Token = token,
                        AccountId = existingOAuthUser.UserId.ToString()
                    }));
                }

                // Check if user exists with same email but different provider
                var existingEmailUser = _userRepo.GetUserByEmail(email);
                if (existingEmailUser != null)
                {
                    // Link OAuth account to existing user
                    existingEmailUser.OAuthProvider = provider;
                    existingEmailUser.OAuthId = oauthId;
                    existingEmailUser.OAuthEmail = email;
                    _userRepo.UpdateUser(existingEmailUser);

                    var token = GenerateJwtToken(existingEmailUser);
                    return Task.FromResult<IActionResult>(Ok(new LoginResponse
                    {
                        Role = existingEmailUser.RoleId?.ToString() ?? "Member",
                        Token = token,
                        AccountId = existingEmailUser.UserId.ToString()
                    }));
                }

                // Create new user
                var newUser = _userRepo.CreateOAuthUser(provider, oauthId, email, fullName, avatar);
                var newToken = GenerateJwtToken(newUser);
                
                return Task.FromResult<IActionResult>(Ok(new LoginResponse
                {
                    Role = newUser.RoleId?.ToString() ?? "Member",
                    Token = newToken,
                    AccountId = newUser.UserId.ToString()
                }));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IActionResult>(StatusCode(500, new { message = "Internal server error: " + ex.Message }));
            }
        }

        [HttpGet("oauth-error")]
        [AllowAnonymous]
        public ActionResult OAuthError([FromQuery] string? message)
        {
            return Ok(new
            {
                success = false,
                message = message ?? "OAuth authentication failed",
                timestamp = DateTime.Now
            });
        }

        [HttpGet("test-oauth")]
        [AllowAnonymous]
        public ActionResult TestOAuth()
        {
            return Ok(new
            {
                message = "OAuth test endpoint",
                googleLoginUrl = "/api/User/google-login",
                facebookLoginUrl = "/api/User/facebook-login",
                instructions = "Click on the URLs above to test OAuth login"
            });
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

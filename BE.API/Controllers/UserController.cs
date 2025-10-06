using BE.API.DTOs.Request;
using BE.API.DTOs.Response;
using BE.BOs.Models;
using BE.REPOs.Interface;
using BE.REPOs.Service;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
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

        public UserController(IUserRepo userRepo, IConfiguration configuration, CloudinaryService cloudinaryService)
        {
            _userRepo = userRepo;
            _configuration = configuration;
            _cloudinary = cloudinaryService;
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
                Role = user.RoleId.ToString(),
                Token = token,
                AccountId = user.UserId.ToString()
            });
        }

        private string GenerateJwtToken(User user)
        {
            IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, true).Build();

            var claims = new List<Claim> {
            new Claim(ClaimTypes.Name,user.FullName),
            new Claim(ClaimTypes.Email,user.Email),
            new Claim("UserId",user.UserId.ToString())
            };

            if (user.RoleId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.Role, user.RoleId.Value.ToString()));
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var preparedToken = new JwtSecurityToken(
                            issuer: configuration["JWT:Issuer"],
                            audience: configuration["JWT:Audience"],
                            claims: claims,
                            expires: DateTime.Now.AddMinutes(30),
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
    }
}

using BE.API.DTOs.Request;
using BE.API.DTOs.Response;
using BE.BOs.Models;
using BE.REPOs.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BE.API.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepo _userRepo;

        public UserController(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpPost("api/login")]
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
        {
            var user = _userRepo.GetAccountByEmailAndPassword(request.Email, request.Password);
            if (user == null)
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
            return Ok(user);
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

    }
}

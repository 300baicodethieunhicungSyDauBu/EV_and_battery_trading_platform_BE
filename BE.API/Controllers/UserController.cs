using BE.BOs.Models;
using BE.REPOs.Interface;
using Microsoft.AspNetCore.Mvc;

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
        public ActionResult<User> Login([FromBody] LoginRequest request)
        {
            var user = _userRepo.GetAccountByEmailAndPassword(request.Email, request.Password);
            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }
            return Ok(user);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

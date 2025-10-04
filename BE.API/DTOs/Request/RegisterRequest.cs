namespace BE.API.DTOs.Request
{
    public class RegisterRequest
    {
        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string? FullName { get; set; }

        public string? Phone { get; set; }

        public string? Avatar { get; set; }
    }
}

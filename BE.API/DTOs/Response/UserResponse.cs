namespace BE.API.DTOs.Response
{
    public class UserResponse
    {
        public int UserId { get; set; }

        public string Role { get; set; }

        public string Email { get; set; } = null!;

        public string? FullName { get; set; }

        public string? Phone { get; set; }

        public string? Avatar { get; set; }

        public string? AccountStatus { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}

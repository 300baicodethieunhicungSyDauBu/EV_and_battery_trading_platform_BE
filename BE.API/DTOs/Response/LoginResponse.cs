namespace BE.API.DTOs.Response
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
    }
}

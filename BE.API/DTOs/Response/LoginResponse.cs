namespace BE.API.DTOs.Response
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string Role { get; set; }
        public string AccountId { get; set; }
    }
}

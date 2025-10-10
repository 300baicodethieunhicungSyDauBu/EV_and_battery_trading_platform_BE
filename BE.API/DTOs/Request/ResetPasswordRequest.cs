namespace BE.API.DTOs.Request
{
    public class ResetPasswordRequest
    {
        public string Token { get; set; } = string.Empty; // OTP (6 digits)
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

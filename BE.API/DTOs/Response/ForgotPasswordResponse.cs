namespace BE.API.DTOs.Response
{
    public class ForgotPasswordResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ResetToken { get; set; } // Chỉ trả về trong development
    }
}

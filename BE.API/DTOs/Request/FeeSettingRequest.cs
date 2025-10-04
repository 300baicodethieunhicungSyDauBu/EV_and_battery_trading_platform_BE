namespace BE.API.DTOs.Request
{
    public class FeeSettingRequest
    {
        public string FeeType { get; set; } = string.Empty;
        public decimal FeeValue { get; set; }
        public bool IsActive { get; set; }
    }
}

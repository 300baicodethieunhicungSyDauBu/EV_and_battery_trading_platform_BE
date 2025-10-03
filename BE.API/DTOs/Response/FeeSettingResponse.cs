namespace BE.API.DTOs.Response
{
    public class FeeSettingResponse
    {
        public int FeeId { get; set; }
        public string FeeType { get; set; } = string.Empty;
        public decimal FeeValue { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}

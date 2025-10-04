namespace BE.API.DTOs.Request
{
    public class ReportedListingRequest
    {
        public int ProductId { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public string ReportReason { get; set; } = string.Empty;
    }
}

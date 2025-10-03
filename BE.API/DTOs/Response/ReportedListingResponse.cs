namespace BE.API.DTOs.Response
{
    public class ReportedListingResponse
    {
        public int ReportId { get; set; }
        public int ProductId { get; set; }
        public string ProductTitle { get; set; } = string.Empty;
        public int ReporterId { get; set; }
        public string ReporterName { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string ReportReason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
    }
}

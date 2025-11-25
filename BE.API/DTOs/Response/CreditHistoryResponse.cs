namespace BE.API.DTOs.Response
{
    public class CreditHistoryResponse
    {
        public int HistoryId { get; set; }
        public int UserId { get; set; }
        public string? UserEmail { get; set; }
        public int? PaymentId { get; set; }
        public int? ProductId { get; set; }
        public string? ProductTitle { get; set; }
        public string ChangeType { get; set; } = null!;
        public int CreditsBefore { get; set; }
        public int CreditsChanged { get; set; }
        public int CreditsAfter { get; set; }
        public string? Reason { get; set; }
        public int? CreatedBy { get; set; }
        public string? AdminName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

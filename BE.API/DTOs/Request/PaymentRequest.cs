namespace BE.API.DTOs.Request
{
    public class PaymentRequest
    {
        public int? OrderId { get; set; }
        public int? ProductId { get; set; }

        public int? PayerId { get; set; }

        public string? PaymentType { get; set; }

        public decimal Amount { get; set; }
        
        public int? PostCredits { get; set; }
        
    }
}

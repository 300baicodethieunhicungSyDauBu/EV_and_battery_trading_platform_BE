namespace BE.API.DTOs.Response
{
    public class PaymentResponse
    {
        public int PaymentId { get; set; }

        public int? OrderId { get; set; }

        public int? PayerId { get; set; }

        public string? PaymentType { get; set; }

        public decimal Amount { get; set; }

        public string? PaymentMethod { get; set; }

        public string? Status { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}

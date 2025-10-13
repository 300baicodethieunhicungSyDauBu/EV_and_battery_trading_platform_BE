namespace BE.API.DTOs.Request
{
    public class OrderRequest
    {
        public int? BuyerId { get; set; }

        public int? SellerId { get; set; }

        public int? ProductId { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal DepositAmount { get; set; }

        public string? Status { get; set; }

        public string? DepositStatus { get; set; }

        public string? FinalPaymentStatus { get; set; }

        public DateTime? FinalPaymentDueDate { get; set; }

        public decimal? PayoutAmount { get; set; }

        public string? PayoutStatus { get; set; }

    }
}

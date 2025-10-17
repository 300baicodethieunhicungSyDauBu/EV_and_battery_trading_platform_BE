namespace BE.API.DTOs.Request
{
    public class PaymentRequest
    {
        public int? OrderId { get; set; }
        public int? ProductId { get; set; }

        public int? PayerId { get; set; }

        public string? PaymentType { get; set; }

        public decimal Amount { get; set; }

        //public string? PaymentMethod { get; set; }

        //public string? TransactionNo { get; set; }

        //public string? BankCode { get; set; }

        //public string? BankTranNo { get; set; }

        //public string? CardType { get; set; }

        //public DateTime? PayDate { get; set; }

        //public string? ResponseCode { get; set; }

        //public string? TransactionStatus { get; set; }

        //public string? SecureHash { get; set; }
    }
}

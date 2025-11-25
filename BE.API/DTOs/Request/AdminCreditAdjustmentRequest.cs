namespace BE.API.DTOs.Request
{
    public class AdminCreditAdjustmentRequest
    {
        public int UserId { get; set; }
        
        public int CreditsChange { get; set; }  // Positive = add, Negative = subtract
        
        public string Reason { get; set; } = null!;
        
        public string AdjustmentType { get; set; } = "Correction";  // "Refund", "Promotion", "Correction", "Penalty"
    }
}

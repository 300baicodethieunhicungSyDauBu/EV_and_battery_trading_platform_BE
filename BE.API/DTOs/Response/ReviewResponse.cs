namespace BE.API.DTOs.Response
{
    public class ReviewResponse
    {
        public int ReviewId { get; set; }
        public int OrderId { get; set; }
        public int ReviewerId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public int RevieweeId { get; set; }
        public string RevieweeName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
    }
}

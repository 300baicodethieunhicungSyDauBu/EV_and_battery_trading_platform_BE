namespace BE.API.DTOs.Response
{
    public class BatteryResponse
    {
        public int ProductId { get; set; }
        public int? SellerId { get; set; }
        public string? ProductType { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Condition { get; set; }
        
        // Các trường cụ thể cho pin
        public string? BatteryType { get; set; }
        public decimal? BatteryHealth { get; set; }
        public decimal? Capacity { get; set; }
        public decimal? Voltage { get; set; }
        public string? BMS { get; set; }
        public string? CellType { get; set; }
        public int? CycleCount { get; set; }
        
        // Thông tin cơ bản
        public string? Status { get; set; }
        public string? VerificationStatus { get; set; }
        public DateTime? CreatedDate { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}

namespace BE.API.DTOs.Response
{
    public class VehicleResponse
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
        
        // Các trường cụ thể cho xe
        public string? VehicleType { get; set; }
        public int? ManufactureYear { get; set; }
        public int? Mileage { get; set; }
        public string? Transmission { get; set; }
        public int? SeatCount { get; set; }
        public string? LicensePlate { get; set; }
        
        // Thông tin cơ bản
        public string? Status { get; set; }
        public string? VerificationStatus { get; set; }
        public DateTime? CreatedDate { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}

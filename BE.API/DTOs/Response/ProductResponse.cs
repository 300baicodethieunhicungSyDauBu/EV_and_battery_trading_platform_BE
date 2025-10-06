namespace BE.API.DTOs.Response
{
    public class ProductResponse
    {
        public int ProductId { get; set; }
        public int? SellerId { get; set; }

        public string ProductType { get; set; } = null!;

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string Brand { get; set; } = null!;

        public string? Model { get; set; }

        public string? Condition { get; set; }

        public string? VehicleType { get; set; }

        public int? ManufactureYear { get; set; }

        public int? Mileage { get; set; }

        public decimal? BatteryHealth { get; set; }

        public string? BatteryType { get; set; }

        public decimal? Capacity { get; set; }

        public decimal? Voltage { get; set; }

        public int? CycleCount { get; set; }

        public string? Status { get; set; }

        public string? VerificationStatus { get; set; }

        public DateTime? CreatedDate { get; set; }

        public List<string> ImageUrls { get; set; } = new();
    }
}

using System.ComponentModel.DataAnnotations;

namespace BE.API.DTOs.Request
{
    public class VehicleRequest
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [StringLength(100, ErrorMessage = "Brand cannot exceed 100 characters")]
        public string? Brand { get; set; }

        [StringLength(100, ErrorMessage = "Model cannot exceed 100 characters")]
        public string? Model { get; set; }

        [StringLength(50, ErrorMessage = "Condition cannot exceed 50 characters")]
        public string? Condition { get; set; }

        // Các trường cụ thể cho xe
        [Required(ErrorMessage = "Vehicle type is required")]
        [StringLength(50, ErrorMessage = "Vehicle type cannot exceed 50 characters")]
        public string VehicleType { get; set; } = null!;

        [Range(1900, 2030, ErrorMessage = "Manufacture year must be between 1900 and 2030")]
        public int? ManufactureYear { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Mileage must be greater than or equal to 0")]
        public int? Mileage { get; set; }

        [StringLength(50, ErrorMessage = "Transmission cannot exceed 50 characters")]
        public string? Transmission { get; set; }

        [Range(1, 50, ErrorMessage = "Seat count must be between 1 and 50")]
        public int? SeatCount { get; set; }

        [StringLength(20, ErrorMessage = "License plate cannot exceed 20 characters")]
        public string? LicensePlate { get; set; }
    }
}

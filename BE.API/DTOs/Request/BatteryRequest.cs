using System.ComponentModel.DataAnnotations;

namespace BE.API.DTOs.Request
{
    public class BatteryRequest
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

        // Các trường cụ thể cho pin
        [Required(ErrorMessage = "Battery type is required")]
        [StringLength(50, ErrorMessage = "Battery type cannot exceed 50 characters")]
        public string BatteryType { get; set; } = null!;

        [Range(0, 100, ErrorMessage = "Battery health must be between 0 and 100")]
        public decimal? BatteryHealth { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Capacity must be greater than 0")]
        public decimal? Capacity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Voltage must be greater than 0")]
        public decimal? Voltage { get; set; }

        [StringLength(100, ErrorMessage = "BMS cannot exceed 100 characters")]
        public string? BMS { get; set; }

        [StringLength(50, ErrorMessage = "Cell type cannot exceed 50 characters")]
        public string? CellType { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Cycle count must be greater than or equal to 0")]
        public int? CycleCount { get; set; }
    }
}

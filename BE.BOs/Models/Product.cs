using System;
using System.Collections.Generic;

namespace BE.BOs.Models;

public partial class Product
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

    public string? Transmission { get; set; }

    public int? SeatCount { get; set; }

    public decimal? BatteryHealth { get; set; }

    public string? BatteryType { get; set; }

    public decimal? Capacity { get; set; }

    public decimal? Voltage { get; set; }

    public string? BMS { get; set; }

    public string? CellType { get; set; }

    public int? CycleCount { get; set; }

    public string? LicensePlate { get; set; }

    public string? WarrantyPeriod { get; set; }

    public string? Status { get; set; }

    public string? VerificationStatus { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<ReportedListing> ReportedListings { get; set; } = new List<ReportedListing>();

    public virtual User? Seller { get; set; }
}

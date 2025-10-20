namespace BE.API.DTOs.Request;

public class ProductSearchRequest
{
    public string? Keyword { get; set; }
    
    //filter chung
    public string? ProductType { get; set; } 
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Condition { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    // filter xe
    public string? VehicleType { get; set; }
    public string? Transmission { get; set; }
    public int? MinManufactureYear { get; set; }
    public int? MaxManufactureYear { get; set; }
    public int? MaxMileage { get; set; }
    public int? SeatCount { get; set; }

    // filter pin
    public string? BatteryType { get; set; }
    public decimal? MinBatteryHealth { get; set; }
    public decimal? MaxBatteryHealth { get; set; }
    public decimal? MinCapacity { get; set; }
    public decimal? MaxCapacity { get; set; }
    public string? CellType { get; set; }
    public string? BMS { get; set; }

    //filter trạng thái
    public string? Status { get; set; } 
    public string? VerificationStatus { get; set; }
}
namespace BE.BOs.Models;

public class PostCreditPackage
{
    public string PackageId { get; set; } = null!; // "PostCredit_5"
    
    public int Credits { get; set; } // 5
    
    public decimal Price { get; set; } // 50000
    
    public decimal PricePerCredit { get; set; } // 10000
    
    public int DiscountPercent { get; set; } // 0%
    
    public bool IsPopular { get; set; } // false
    
    public string? Description { get; set; }
}

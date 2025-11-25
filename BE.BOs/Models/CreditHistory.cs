using System;

namespace BE.BOs.Models;

public partial class CreditHistory
{
    public int HistoryId { get; set; }

    public int UserId { get; set; }

    public int? PaymentId { get; set; }

    public int? ProductId { get; set; }

    public string ChangeType { get; set; } = null!; // Purchase, Use, Refund, AdminAdjust

    public int CreditsBefore { get; set; }

    public int CreditsChanged { get; set; } // Positive for add, negative for use

    public int CreditsAfter { get; set; }

    public string? Reason { get; set; }

    public int? CreatedBy { get; set; } // Admin user ID if manual adjustment

    public DateTime CreatedDate { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;

    public virtual Payment? Payment { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? CreatedByUser { get; set; }
}

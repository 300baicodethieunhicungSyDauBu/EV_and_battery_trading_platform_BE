using System;
using System.Collections.Generic;

namespace BE.BOs.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? BuyerId { get; set; }

    public int? SellerId { get; set; }

    public int? ProductId { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal DepositAmount { get; set; }

    public string? Status { get; set; }

    public string? DepositStatus { get; set; }

    public string? FinalPaymentStatus { get; set; }

    public decimal? PayoutAmount { get; set; }

    public string? PayoutStatus { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public virtual User? Buyer { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Product? Product { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual User? Seller { get; set; }
}

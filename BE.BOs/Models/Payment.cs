using System;
using System.Collections.Generic;

namespace BE.BOs.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int? OrderId { get; set; }
    public int? ProductId { get; set; }

    public int? PayerId { get; set; }

    public string? PaymentType { get; set; }

    public decimal Amount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Order? Order { get; set; }
    public virtual Product? Product { get; set; }

    public virtual User? Payer { get; set; }
}

using System;
using System.Collections.Generic;

namespace BE.BOs.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int? OrderId { get; set; }

    public int? ReviewerId { get; set; }

    public int? RevieweeId { get; set; }

    public int? Rating { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Order? Order { get; set; }

    public virtual User? Reviewee { get; set; }

    public virtual User? Reviewer { get; set; }
}

using System;
using System.Collections.Generic;

namespace BE.BOs.Models;

public partial class FeeSetting
{
    public int FeeId { get; set; }

    public string FeeType { get; set; } = null!;

    public decimal FeeValue { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }
}

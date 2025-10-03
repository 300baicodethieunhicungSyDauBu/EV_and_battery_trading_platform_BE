using System;
using System.Collections.Generic;

namespace BE.BOs.Models;

public partial class ReportedListing
{
    public int ReportId { get; set; }

    public int? ProductId { get; set; }

    public int? ReporterId { get; set; }

    public string? ReportType { get; set; }

    public string ReportReason { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? Reporter { get; set; }
}

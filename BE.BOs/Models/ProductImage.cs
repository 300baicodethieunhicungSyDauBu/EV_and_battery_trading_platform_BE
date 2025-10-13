using System;
using System.Collections.Generic;

namespace BE.BOs.Models;

public partial class ProductImage
{
    public int ImageId { get; set; }

    public int? ProductId { get; set; }

    public string? Name { get; set; }

    public string ImageData { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public virtual Product? Product { get; set; }
}

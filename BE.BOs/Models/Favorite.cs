using System;
using System.Collections.Generic;

namespace BE.BOs.Models;

public partial class Favorite
{
    public int FavoriteId { get; set; }

    public int? UserId { get; set; }

    public int? ProductId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? User { get; set; }
}

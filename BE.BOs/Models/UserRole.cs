using System;
using System.Collections.Generic;

namespace BE.BOs.Models;

public partial class UserRole
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}

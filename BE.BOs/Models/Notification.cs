using System;
using System.Collections.Generic;

namespace BE.BOs.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int? UserId { get; set; }

    public string? NotificationType { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual User? User { get; set; }
}

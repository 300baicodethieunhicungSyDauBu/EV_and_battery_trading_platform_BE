using System;

namespace BE.BOs.Models;

public partial class Message
{
    public int MessageId { get; set; }

    public int? ChatId { get; set; }

    public int? SenderId { get; set; }

    public string Content { get; set; } = null!;

    public bool? IsRead { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Chat? Chat { get; set; }

    public virtual User? Sender { get; set; }
}

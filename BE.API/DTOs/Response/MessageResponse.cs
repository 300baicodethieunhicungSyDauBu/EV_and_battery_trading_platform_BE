using BE.API.DTOs.Response;

namespace BE.API.DTOs.Response;

public class MessageResponse
{
    public int MessageId { get; set; }
    public int? ChatId { get; set; }
    public int? SenderId { get; set; }
    public string Content { get; set; } = null!;
    public bool? IsRead { get; set; }
    public DateTime? CreatedDate { get; set; }
    public UserResponse? Sender { get; set; }
    public ChatResponse? Chat { get; set; }
}

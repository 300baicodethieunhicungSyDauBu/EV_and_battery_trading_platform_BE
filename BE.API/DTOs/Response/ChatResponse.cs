using BE.API.DTOs.Response;

namespace BE.API.DTOs.Response;

public class ChatResponse
{
    public int ChatId { get; set; }
    public int? User1Id { get; set; }
    public int? User2Id { get; set; }
    public DateTime? CreatedDate { get; set; }
    public UserResponse? User1 { get; set; }
    public UserResponse? User2 { get; set; }
    public List<MessageResponse>? Messages { get; set; }
    public int? UnreadCount { get; set; }
    public MessageResponse? LastMessage { get; set; }
}

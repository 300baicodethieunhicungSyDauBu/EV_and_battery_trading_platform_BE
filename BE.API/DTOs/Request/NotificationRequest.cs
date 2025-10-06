namespace BE.API.DTOs.Request
{
    public class NotificationRequest
    {
        public int UserId { get; set; }
        public string NotificationType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}

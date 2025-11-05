namespace BE.API.DTOs.Response
{
    public class NotificationResponse
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public bool IsRead { get; set; } = false;
    }
}

using BE.BOs.Models;

namespace BE.REPOs.Interface
{
    public interface INotificationsRepo
    {
        List<Notification> GetAllNotifications();
        Notification? GetNotificationById(int notificationId);
        List<Notification> GetNotificationsByUserId(int userId);
        List<Notification> GetNotificationsByType(string type);
        Notification CreateNotification(Notification notification);
        Notification UpdateNotification(Notification notification);
        bool DeleteNotification(int notificationId);
        bool MarkNotificationAsRead(int notificationId);
    }
}

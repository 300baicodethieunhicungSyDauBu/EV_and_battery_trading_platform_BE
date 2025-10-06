using BE.BOs.Models;
using BE.DAOs;
using BE.REPOs.Interface;

namespace BE.REPOs.Implementation
{
    public class NotificationsRepo : INotificationsRepo
    {
        public List<Notification> GetAllNotifications()
        {
            return NotificationsDAO.Instance.GetAllNotifications();
        }

        public Notification? GetNotificationById(int notificationId)
        {
            return NotificationsDAO.Instance.GetNotificationById(notificationId);
        }

        public List<Notification> GetNotificationsByUserId(int userId)
        {
            return NotificationsDAO.Instance.GetNotificationsByUserId(userId);
        }

        public List<Notification> GetNotificationsByType(string type)
        {
            return NotificationsDAO.Instance.GetNotificationsByType(type);
        }

        public Notification CreateNotification(Notification notification)
        {
            return NotificationsDAO.Instance.CreateNotification(notification);
        }

        public Notification UpdateNotification(Notification notification)
        {
            return NotificationsDAO.Instance.UpdateNotification(notification);
        }

        public bool DeleteNotification(int notificationId)
        {
            return NotificationsDAO.Instance.DeleteNotification(notificationId);
        }
    }
}

using BE.BOs.Models;
using Microsoft.EntityFrameworkCore;

namespace BE.DAOs
{
    public class NotificationsDAO
    {
        private static NotificationsDAO? instance;
        private static readonly object lockObject = new object();

        public static NotificationsDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new NotificationsDAO();
                        }
                    }
                }
                return instance;
            }
        }

        private NotificationsDAO() { }

        public List<Notification> GetAllNotifications()
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.Notifications
                .Include(n => n.User)
                .ToList();
        }

        public Notification? GetNotificationById(int notificationId)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.Notifications
                .Include(n => n.User)
                .FirstOrDefault(n => n.NotificationId == notificationId);
        }

        public List<Notification> GetNotificationsByUserId(int userId)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.Notifications
                .Include(n => n.User)
                .Where(n => n.UserId == userId)
                .ToList();
        }

        public List<Notification> GetNotificationsByType(string type)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.Notifications
                .Include(n => n.User)
                .Where(n => n.NotificationType == type)
                .ToList();
        }

        public Notification CreateNotification(Notification notification)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            notification.CreatedDate = DateTime.Now;
            notification.IsRead = false;
            context.Notifications.Add(notification);
            context.SaveChanges();
            return notification;
        }

        public Notification UpdateNotification(Notification notification)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            var existingNotification = context.Notifications.FirstOrDefault(n => n.NotificationId == notification.NotificationId);
            if (existingNotification != null)
            {
                existingNotification.NotificationType = notification.NotificationType;
                existingNotification.Title = notification.Title;
                existingNotification.Content = notification.Content;
                context.SaveChanges();
                return existingNotification;
            }
            return notification;
        }

        public bool DeleteNotification(int notificationId)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            var notification = context.Notifications.FirstOrDefault(n => n.NotificationId == notificationId);
            if (notification != null)
            {
                context.Notifications.Remove(notification);
                context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool MarkNotificationAsRead(int notificationId)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            var notification = context.Notifications.FirstOrDefault(n => n.NotificationId == notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}


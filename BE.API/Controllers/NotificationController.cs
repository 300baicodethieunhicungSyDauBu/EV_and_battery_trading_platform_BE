using BE.API.DTOs.Request;
using BE.API.DTOs.Response;
using BE.BOs.Models;
using BE.REPOs.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationsRepo _notificationsRepo;

        public NotificationController(INotificationsRepo notificationsRepo)
        {
            _notificationsRepo = notificationsRepo;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult<IEnumerable<NotificationResponse>> GetAllNotifications()
        {
            try
            {
                var notifications = _notificationsRepo.GetAllNotifications();
                var response = notifications.Select(notification => new NotificationResponse
                {
                    NotificationId = notification.NotificationId,
                    UserId = notification.UserId ?? 0,
                    UserName = notification.User?.FullName ?? "Unknown",
                    NotificationType = notification.NotificationType ?? "",
                    Title = notification.Title,
                    Content = notification.Content ?? "",
                    CreatedDate = notification.CreatedDate,
                    IsRead = notification.IsRead
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("{id}")]
        public ActionResult<NotificationResponse> GetNotificationById(int id)
        {
            try
            {
                var notification = _notificationsRepo.GetNotificationById(id);
                if (notification == null)
                {
                    return NotFound("Notification not found");
                }

                var response = new NotificationResponse
                {
                    NotificationId = notification.NotificationId,
                    UserId = notification.UserId ?? 0,
                    UserName = notification.User?.FullName ?? "Unknown",
                    NotificationType = notification.NotificationType ?? "",
                    Title = notification.Title,
                    Content = notification.Content ?? "",
                    CreatedDate = notification.CreatedDate,
                    IsRead = notification.IsRead
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // XEM THÔNG BÁO CỦA USER (Member/Admin)
        // Input: userId
        // Output: Danh sách notifications của user đó
        // Auth: Chỉ xem được notifications của mình (trừ admin)
        [HttpGet("user/{userId}")]
        [Authorize]
        public ActionResult<IEnumerable<NotificationResponse>> GetNotificationsByUserId(int userId)
        {
            try
            {
                // Lấy userId từ token
                var currentUserIdStr = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(currentUserIdStr) || !int.TryParse(currentUserIdStr, out var currentUserId))
                {
                    return Unauthorized("Invalid user token");
                }

                // Kiểm tra quyền (chỉ xem được notifications của mình, trừ admin)
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
                if (currentUserId != userId && userRole != "1")
                {
                    return Forbid("You can only view your own notifications");
                }

                // Lấy danh sách notifications
                var notifications = _notificationsRepo.GetNotificationsByUserId(userId);
                var response = notifications.Select(notification => new NotificationResponse
                {
                    NotificationId = notification.NotificationId,
                    UserId = notification.UserId ?? 0,
                    UserName = notification.User?.FullName ?? "Unknown",
                    NotificationType = notification.NotificationType ?? "",
                    Title = notification.Title,
                    Content = notification.Content ?? "",
                    CreatedDate = notification.CreatedDate,
                    IsRead = notification.IsRead
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("type/{type}")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult<IEnumerable<NotificationResponse>> GetNotificationsByType(string type)
        {
            try
            {
                var notifications = _notificationsRepo.GetNotificationsByType(type);
                var response = notifications.Select(notification => new NotificationResponse
                {
                    NotificationId = notification.NotificationId,
                    UserId = notification.UserId ?? 0,
                    UserName = notification.User?.FullName ?? "Unknown",
                    NotificationType = notification.NotificationType ?? "",
                    Title = notification.Title,
                    Content = notification.Content ?? "",
                    CreatedDate = notification.CreatedDate,
                    IsRead = notification.IsRead
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult<NotificationResponse> CreateNotification([FromBody] NotificationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Title))
                {
                    return BadRequest("Title is required");
                }

                if (request.UserId <= 0)
                {
                    return BadRequest("Valid user ID is required");
                }

                var notification = new Notification
                {
                    UserId = request.UserId,
                    NotificationType = request.NotificationType,
                    Title = request.Title,
                    Content = request.Content
                };

                var createdNotification = _notificationsRepo.CreateNotification(notification);

                var response = new NotificationResponse
                {
                    NotificationId = createdNotification.NotificationId,
                    UserId = createdNotification.UserId ?? 0,
                    UserName = createdNotification.User?.FullName ?? "Unknown",
                    NotificationType = createdNotification.NotificationType ?? "",
                    Title = createdNotification.Title,
                    Content = createdNotification.Content ?? "",
                    CreatedDate = createdNotification.CreatedDate,
                    IsRead = createdNotification.IsRead
                };

                return CreatedAtAction(nameof(GetNotificationById), new { id = createdNotification.NotificationId }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult<NotificationResponse> UpdateNotification(int id, [FromBody] NotificationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Title))
                {
                    return BadRequest("Title is required");
                }

                var existingNotification = _notificationsRepo.GetNotificationById(id);
                if (existingNotification == null)
                {
                    return NotFound("Notification not found");
                }

                existingNotification.NotificationType = request.NotificationType;
                existingNotification.Title = request.Title;
                existingNotification.Content = request.Content;

                var updatedNotification = _notificationsRepo.UpdateNotification(existingNotification);

                var response = new NotificationResponse
                {
                    NotificationId = updatedNotification.NotificationId,
                    UserId = updatedNotification.UserId ?? 0,
                    UserName = updatedNotification.User?.FullName ?? "Unknown",
                    NotificationType = updatedNotification.NotificationType ?? "",
                    Title = updatedNotification.Title,
                    Content = updatedNotification.Content ?? "",
                    CreatedDate = updatedNotification.CreatedDate,
                    IsRead = updatedNotification.IsRead
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("{id}/read")]
        [Authorize]
        public ActionResult MarkNotificationAsRead(int id)
        {
            try
            {
                var userIdStr = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    return Unauthorized("Invalid user token");
                }

                var notification = _notificationsRepo.GetNotificationById(id);
                if (notification == null)
                {
                    return NotFound("Notification not found");
                }

                // Verify user owns this notification
                if (notification.UserId != userId)
                {
                    return Forbid("You can only mark your own notifications as read");
                }

                var marked = _notificationsRepo.MarkNotificationAsRead(id);
                if (marked)
                {
                    return Ok(new { message = "Notification marked as read" });
                }
                else
                {
                    return StatusCode(500, "Failed to mark notification as read");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("mark-all-read")]
        [Authorize]
        public ActionResult MarkAllNotificationsAsRead()
        {
            try
            {
                var userIdStr = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    return Unauthorized("Invalid user token");
                }

                var notifications = _notificationsRepo.GetNotificationsByUserId(userId);
                var unreadNotifications = notifications.Where(n => !n.IsRead).ToList();

                if (!unreadNotifications.Any())
                {
                    return Ok(new { message = "No unread notifications", count = 0 });
                }

                int markedCount = 0;
                foreach (var notification in unreadNotifications)
                {
                    if (_notificationsRepo.MarkNotificationAsRead(notification.NotificationId))
                    {
                        markedCount++;
                    }
                }

                return Ok(new { message = "All notifications marked as read", count = markedCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public ActionResult DeleteNotification(int id)
        {
            try
            {
                var userIdStr = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    return Unauthorized("Invalid user token");
                }

                var notification = _notificationsRepo.GetNotificationById(id);
                if (notification == null)
                {
                    return NotFound("Notification not found");
                }

                // User chỉ xóa được notification của mình, Admin xóa được tất cả
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
                if (notification.UserId != userId && userRole != "1")
                {
                    return Forbid("You can only delete your own notifications");
                }

                var result = _notificationsRepo.DeleteNotification(id);
                if (!result)
                {
                    return StatusCode(500, "Failed to delete notification");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}

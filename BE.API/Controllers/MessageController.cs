using BE.API.DTOs.Request;
using BE.API.DTOs.Response;
using BE.BOs.Models;
using BE.REPOs.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BE.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BE.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController : ControllerBase
{
    private readonly IMessageRepo _messageRepo;
    private readonly IChatRepo _chatRepo;
    private readonly IHubContext<ChatHub> _hubContext;

    public MessageController(IMessageRepo messageRepo, IChatRepo chatRepo, IHubContext<ChatHub> hubContext)
    {
        _messageRepo = messageRepo;
        _chatRepo = chatRepo;
        _hubContext = hubContext;
    }

    [HttpGet]
    [Authorize(Policy = "MemberOnly")]
    public ActionResult<List<MessageResponse>> GetAllMessages()
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var messages = _messageRepo.GetMessagesBySenderId(userId);
            
            var messageResponses = messages.Select(message => new MessageResponse
            {
                MessageId = message.MessageId,
                ChatId = message.ChatId,
                SenderId = message.SenderId,
                Content = message.Content,
                IsRead = message.IsRead,
                CreatedDate = message.CreatedDate,
                Sender = message.Sender != null ? new UserResponse
                {
                    UserId = message.Sender.UserId,
                    Email = message.Sender.Email,
                    FullName = message.Sender.FullName,
                    Phone = message.Sender.Phone,
                    Avatar = message.Sender.Avatar,
                    AccountStatus = message.Sender.AccountStatus
                } : null
            }).ToList();

            return Ok(messageResponses);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "MemberOnly")]
    public ActionResult<MessageResponse> GetMessageById(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var message = _messageRepo.GetMessageById(id);
            
            if (message == null)
            {
                return NotFound("Message not found.");
            }

            // Verify user is part of this chat
            var chat = _chatRepo.GetChatById(message.ChatId ?? 0);
            if (chat == null || (chat.User1Id != userId && chat.User2Id != userId))
            {
                return Forbid("You can only access messages from your own chats.");
            }

            var messageResponse = new MessageResponse
            {
                MessageId = message.MessageId,
                ChatId = message.ChatId,
                SenderId = message.SenderId,
                Content = message.Content,
                IsRead = message.IsRead,
                CreatedDate = message.CreatedDate,
                Sender = message.Sender != null ? new UserResponse
                {
                    UserId = message.Sender.UserId,
                    Email = message.Sender.Email,
                    FullName = message.Sender.FullName,
                    Phone = message.Sender.Phone,
                    Avatar = message.Sender.Avatar,
                    AccountStatus = message.Sender.AccountStatus
                } : null
            };

            return Ok(messageResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpGet("chat/{chatId}")]
    [Authorize(Policy = "MemberOnly")]
    public ActionResult<List<MessageResponse>> GetMessagesByChatId(int chatId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            
            // Verify user is part of this chat
            var chat = _chatRepo.GetChatById(chatId);
            if (chat == null || (chat.User1Id != userId && chat.User2Id != userId))
            {
                return Forbid("You can only access messages from your own chats.");
            }

            var messages = _messageRepo.GetMessagesByChatId(chatId);
            
            var messageResponses = messages.Select(message => new MessageResponse
            {
                MessageId = message.MessageId,
                ChatId = message.ChatId,
                SenderId = message.SenderId,
                Content = message.Content,
                IsRead = message.IsRead,
                CreatedDate = message.CreatedDate,
                Sender = message.Sender != null ? new UserResponse
                {
                    UserId = message.Sender.UserId,
                    Email = message.Sender.Email,
                    FullName = message.Sender.FullName,
                    Phone = message.Sender.Phone,
                    Avatar = message.Sender.Avatar,
                    AccountStatus = message.Sender.AccountStatus
                } : null
            }).ToList();

            return Ok(messageResponses);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpGet("unread")]
    [Authorize(Policy = "MemberOnly")]
    public ActionResult<List<MessageResponse>> GetUnreadMessages()
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var messages = _messageRepo.GetUnreadMessagesByUserId(userId);
            
            var messageResponses = messages.Select(message => new MessageResponse
            {
                MessageId = message.MessageId,
                ChatId = message.ChatId,
                SenderId = message.SenderId,
                Content = message.Content,
                IsRead = message.IsRead,
                CreatedDate = message.CreatedDate,
                Sender = message.Sender != null ? new UserResponse
                {
                    UserId = message.Sender.UserId,
                    Email = message.Sender.Email,
                    FullName = message.Sender.FullName,
                    Phone = message.Sender.Phone,
                    Avatar = message.Sender.Avatar,
                    AccountStatus = message.Sender.AccountStatus
                } : null
            }).ToList();

            return Ok(messageResponses);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpPost]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult<MessageResponse>> CreateMessage([FromBody] MessageRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            if (request.SenderId != userId)
                return Forbid("You can only send messages as yourself.");

            var chat = _chatRepo.GetChatById(request.ChatId);
            if (chat == null || (chat.User1Id != userId && chat.User2Id != userId))
                return Forbid("You can only send messages to chats you're part of.");

            // Lưu message
            var message = _messageRepo.CreateMessage(request.ChatId, request.SenderId, request.Content);

            var messageResponse = new MessageResponse
            {
                MessageId = message.MessageId,
                ChatId = message.ChatId,
                SenderId = message.SenderId,
                Content = message.Content,
                IsRead = message.IsRead,
                CreatedDate = message.CreatedDate
            };

            // Notify đến tất cả client trong group (chatId)
            await _hubContext.Clients.Group(request.ChatId.ToString())
                .SendAsync("ReceiveMessage", messageResponse);

            return CreatedAtAction(nameof(GetMessageById), new { id = message.MessageId }, messageResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpPut("{id}/read")]
    [Authorize(Policy = "MemberOnly")]
    public ActionResult MarkMessageAsRead(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var message = _messageRepo.GetMessageById(id);
            
            if (message == null)
            {
                return NotFound("Message not found.");
            }

            // Verify user is part of this chat and not the sender
            var chat = _chatRepo.GetChatById(message.ChatId ?? 0);
            if (chat == null || (chat.User1Id != userId && chat.User2Id != userId))
            {
                return Forbid("You can only mark messages as read from your own chats.");
            }

            if (message.SenderId == userId)
            {
                return BadRequest("Cannot mark your own messages as read.");
            }

            var marked = _messageRepo.MarkMessageAsRead(id);
            if (marked)
            {
                return Ok("Message marked as read.");
            }
            else
            {
                return StatusCode(500, "Failed to mark message as read.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpPut("chat/{chatId}/read-all")]
    [Authorize(Policy = "MemberOnly")]
    public ActionResult MarkAllMessagesAsRead(int chatId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            
            // Verify user is part of this chat
            var chat = _chatRepo.GetChatById(chatId);
            if (chat == null || (chat.User1Id != userId && chat.User2Id != userId))
            {
                return Forbid("You can only mark messages as read from your own chats.");
            }

            var marked = _messageRepo.MarkMessagesAsReadByChatId(chatId, userId);
            if (marked)
            {
                return Ok("All messages marked as read.");
            }
            else
            {
                return StatusCode(500, "Failed to mark messages as read.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "MemberOnly")]
    public ActionResult DeleteMessage(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var message = _messageRepo.GetMessageById(id);
            
            if (message == null)
            {
                return NotFound("Message not found.");
            }

            // Verify user is the sender
            if (message.SenderId != userId)
            {
                return Forbid("You can only delete your own messages.");
            }

            var deleted = _messageRepo.DeleteMessage(id);
            if (deleted)
            {
                return NoContent();
            }
            else
            {
                return StatusCode(500, "Failed to delete message.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpGet("unread-count")]
    [Authorize(Policy = "MemberOnly")]
    public ActionResult<int> GetUnreadMessageCount()
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var count = _messageRepo.GetUnreadMessageCount(userId);
            return Ok(count);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }
}

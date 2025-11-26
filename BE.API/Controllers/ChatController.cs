using BE.API.DTOs.Request;
using BE.API.DTOs.Response;
using BE.BOs.Models;
using BE.REPOs.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BE.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatRepo _chatRepo;
    private readonly IMessageRepo _messageRepo;

    public ChatController(IChatRepo chatRepo, IMessageRepo messageRepo)
    {
        _chatRepo = chatRepo;
        _messageRepo = messageRepo;
    }

    // 💬 XEM TẤT CẢ CHAT CỦA USER (Member only)
    // Output: Danh sách chats với last message và unread count
    [HttpGet]
    [Authorize(Policy = "MemberOnly")]
    public ActionResult<List<ChatResponse>> GetAllChats()
    {
        try
        {
            // 1️⃣ Lấy userId từ token
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            
            // 2️⃣ Lấy tất cả chats của user này
            var chats = _chatRepo.GetChatsByUserId(userId);
            
            var chatResponses = chats.Select(chat => new ChatResponse
            {
                ChatId = chat.ChatId,
                User1Id = chat.User1Id,
                User2Id = chat.User2Id,
                CreatedDate = chat.CreatedDate,
                User1 = chat.User1 != null ? new UserResponse
                {
                    UserId = chat.User1.UserId,
                    Email = chat.User1.Email,
                    FullName = chat.User1.FullName,
                    Phone = chat.User1.Phone,
                    Avatar = chat.User1.Avatar,
                    AccountStatus = chat.User1.AccountStatus
                } : null,
                User2 = chat.User2 != null ? new UserResponse
                {
                    UserId = chat.User2.UserId,
                    Email = chat.User2.Email,
                    FullName = chat.User2.FullName,
                    Phone = chat.User2.Phone,
                    Avatar = chat.User2.Avatar,
                    AccountStatus = chat.User2.AccountStatus
                } : null,
                UnreadCount = _messageRepo.GetUnreadMessageCount(userId),
                LastMessage = chat.Messages?.OrderByDescending(m => m.CreatedDate).FirstOrDefault() != null ? 
                    new MessageResponse
                    {
                        MessageId = chat.Messages.OrderByDescending(m => m.CreatedDate).First().MessageId,
                        Content = chat.Messages.OrderByDescending(m => m.CreatedDate).First().Content,
                        CreatedDate = chat.Messages.OrderByDescending(m => m.CreatedDate).First().CreatedDate,
                        IsRead = chat.Messages.OrderByDescending(m => m.CreatedDate).First().IsRead
                    } : null
            }).ToList();

            return Ok(chatResponses);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    // 🔍 XEM CHI TIẾT 1 CHAT (Member only)
    // Input: chatId
    // Output: Chat detail với messages
    // Auth: Chỉ 2 người trong chat mới xem được
    [HttpGet("{id}")]
    [Authorize(Policy = "MemberOnly")]
    public ActionResult<ChatResponse> GetChatById(int id)
    {
        try
        {
            // 1️⃣ Lấy userId từ token
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            
            // 2️⃣ Lấy chat by ID
            var chat = _chatRepo.GetChatById(id);
            
            if (chat == null)
            {
                return NotFound("Chat not found.");
            }

            // 3️⃣ Kiểm tra quyền (chỉ 2 người trong chat mới xem được)
            if (chat.User1Id != userId && chat.User2Id != userId)
            {
                return Forbid("You can only access your own chats.");
            }

            var chatResponse = new ChatResponse
            {
                ChatId = chat.ChatId,
                User1Id = chat.User1Id,
                User2Id = chat.User2Id,
                CreatedDate = chat.CreatedDate,
                User1 = chat.User1 != null ? new UserResponse
                {
                    UserId = chat.User1.UserId,
                    Email = chat.User1.Email,
                    FullName = chat.User1.FullName,
                    Phone = chat.User1.Phone,
                    Avatar = chat.User1.Avatar,
                    AccountStatus = chat.User1.AccountStatus
                } : null,
                User2 = chat.User2 != null ? new UserResponse
                {
                    UserId = chat.User2.UserId,
                    Email = chat.User2.Email,
                    FullName = chat.User2.FullName,
                    Phone = chat.User2.Phone,
                    Avatar = chat.User2.Avatar,
                    AccountStatus = chat.User2.AccountStatus
                } : null,
                Messages = chat.Messages?.Select(m => new MessageResponse
                {
                    MessageId = m.MessageId,
                    ChatId = m.ChatId,
                    SenderId = m.SenderId,
                    Content = m.Content,
                    IsRead = m.IsRead,
                    CreatedDate = m.CreatedDate,
                    Sender = m.Sender != null ? new UserResponse
                    {
                        UserId = m.Sender.UserId,
                        Email = m.Sender.Email,
                        FullName = m.Sender.FullName,
                        Phone = m.Sender.Phone,
                        Avatar = m.Sender.Avatar,
                        AccountStatus = m.Sender.AccountStatus
                    } : null
                }).ToList()
            };

            return Ok(chatResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpPost]
    [Authorize(Policy = "MemberOnly")]
    public ActionResult<ChatResponse> CreateChat([FromBody] ChatRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            
            // Verify user is one of the participants
            if (request.User1Id != userId && request.User2Id != userId)
            {
                return Forbid("You can only create chats involving yourself.");
            }

            // Check if chat already exists
            var existingChat = _chatRepo.GetChatByUsers(request.User1Id, request.User2Id);
            if (existingChat != null)
            {
                return Conflict("Chat already exists between these users.");
            }

            var chat = _chatRepo.CreateChat(request.User1Id, request.User2Id);
            
            var chatResponse = new ChatResponse
            {
                ChatId = chat.ChatId,
                User1Id = chat.User1Id,
                User2Id = chat.User2Id,
                CreatedDate = chat.CreatedDate
            };

            return CreatedAtAction(nameof(GetChatById), new { id = chat.ChatId }, chatResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpPost("start-chat/{otherUserId}")]
    [Authorize(Policy = "MemberOnly")]
    public ActionResult<ChatResponse> StartChat(int otherUserId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            
            if (userId == otherUserId)
            {
                return BadRequest("Cannot start chat with yourself.");
            }

            // Check if chat already exists
            var existingChat = _chatRepo.GetChatByUsers(userId, otherUserId);
            if (existingChat != null)
            {
                var chatResponse = new ChatResponse
                {
                    ChatId = existingChat.ChatId,
                    User1Id = existingChat.User1Id,
                    User2Id = existingChat.User2Id,
                    CreatedDate = existingChat.CreatedDate
                };
                return Ok(chatResponse);
            }

            var chat = _chatRepo.CreateChat(userId, otherUserId);
            
            var newChatResponse = new ChatResponse
            {
                ChatId = chat.ChatId,
                User1Id = chat.User1Id,
                User2Id = chat.User2Id,
                CreatedDate = chat.CreatedDate
            };

            return CreatedAtAction(nameof(GetChatById), new { id = chat.ChatId }, newChatResponse);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "MemberOnly")]
    public ActionResult DeleteChat(int id)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var chat = _chatRepo.GetChatById(id);
            
            if (chat == null)
            {
                return NotFound("Chat not found.");
            }

            // Verify user is part of this chat
            if (chat.User1Id != userId && chat.User2Id != userId)
            {
                return Forbid("You can only delete your own chats.");
            }

            var deleted = _chatRepo.DeleteChat(id);
            if (deleted)
            {
                return NoContent();
            }
            else
            {
                return StatusCode(500, "Failed to delete chat.");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }
}

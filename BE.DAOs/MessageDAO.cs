using BE.BOs.Models;
using Microsoft.EntityFrameworkCore;

namespace BE.DAOs;

public class MessageDAO
{
    private readonly EvandBatteryTradingPlatformContext _context;

    public MessageDAO(EvandBatteryTradingPlatformContext context)
    {
        _context = context;
    }

    public List<Message> GetAllMessages()
    {
        return _context.Messages
            .Include(m => m.Chat)
            .Include(m => m.Sender)
            .OrderByDescending(m => m.CreatedDate)
            .ToList();
    }

    public Message? GetMessageById(int messageId)
    {
        return _context.Messages
            .Include(m => m.Chat)
            .Include(m => m.Sender)
            .FirstOrDefault(m => m.MessageId == messageId);
    }

    public List<Message> GetMessagesByChatId(int chatId)
    {
        return _context.Messages
            .Include(m => m.Chat)
            .Include(m => m.Sender)
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.CreatedDate)
            .ToList();
    }

    public List<Message> GetMessagesBySenderId(int senderId)
    {
        return _context.Messages
            .Include(m => m.Chat)
            .Include(m => m.Sender)
            .Where(m => m.SenderId == senderId)
            .OrderByDescending(m => m.CreatedDate)
            .ToList();
    }

    public List<Message> GetUnreadMessagesByUserId(int userId)
    {
        return _context.Messages
            .Include(m => m.Chat)
            .Include(m => m.Sender)
            .Where(m => m.Chat!.User1Id == userId || m.Chat!.User2Id == userId)
            .Where(m => m.IsRead == false && m.SenderId != userId)
            .OrderByDescending(m => m.CreatedDate)
            .ToList();
    }

    public Message CreateMessage(int chatId, int senderId, string content)
    {
        var chat = _context.Chats.FirstOrDefault(c => c.ChatId == chatId);
        if (chat == null) throw new Exception("Chat not found");

        var message = new Message
        {
            ChatId = chatId,
            SenderId = senderId,
            Content = content,
            IsRead = false,
            CreatedDate = DateTime.UtcNow // Dùng UTC chuẩn hơn
        };
        _context.Messages.Add(message);
        _context.SaveChanges();
        return message;
    }

    public Message UpdateMessage(Message message)
    {
        _context.Messages.Update(message);
        _context.SaveChanges();
        return message;
    }

    public bool DeleteMessage(int messageId)
    {
        var message = _context.Messages.Find(messageId);
        if (message != null)
        {
            _context.Messages.Remove(message);
            _context.SaveChanges();
            return true;
        }
        return false;
    }

    public bool MarkMessageAsRead(int messageId)
    {
        var message = _context.Messages.Find(messageId);
        if (message != null)
        {
            message.IsRead = true;
            _context.SaveChanges();
            return true;
        }
        return false;
    }

    public bool MarkMessagesAsReadByChatId(int chatId, int userId)
    {
        var messages = _context.Messages
            .Where(m => m.ChatId == chatId && m.SenderId != userId && m.IsRead == false)
            .ToList();

        foreach (var message in messages)
        {
            message.IsRead = true;
        }

        _context.SaveChanges();
        return true;
    }

    public bool MessageExists(int messageId)
    {
        return _context.Messages.Any(m => m.MessageId == messageId);
    }

    public int GetUnreadMessageCount(int userId)
    {
        return _context.Messages
            .Where(m => m.Chat!.User1Id == userId || m.Chat!.User2Id == userId)
            .Where(m => m.IsRead == false && m.SenderId != userId)
            .Count();
    }
}

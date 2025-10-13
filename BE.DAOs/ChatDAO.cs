using BE.BOs.Models;
using Microsoft.EntityFrameworkCore;

namespace BE.DAOs;

public class ChatDAO
{
    private readonly EvandBatteryTradingPlatformContext _context;

    public ChatDAO(EvandBatteryTradingPlatformContext context)
    {
        _context = context;
    }

    public List<Chat> GetAllChats()
    {
        return _context.Chats
            .Include(c => c.User1)
            .Include(c => c.User2)
            .Include(c => c.Messages)
            .OrderByDescending(c => c.CreatedDate)
            .ToList();
    }

    public Chat? GetChatById(int chatId)
    {
        return _context.Chats
            .Include(c => c.User1)
            .Include(c => c.User2)
            .Include(c => c.Messages)
            .FirstOrDefault(c => c.ChatId == chatId);
    }

    public Chat? GetChatByUsers(int user1Id, int user2Id)
    {
        return _context.Chats
            .Include(c => c.User1)
            .Include(c => c.User2)
            .Include(c => c.Messages)
            .FirstOrDefault(c => 
                (c.User1Id == user1Id && c.User2Id == user2Id) ||
                (c.User1Id == user2Id && c.User2Id == user1Id));
    }

    public List<Chat> GetChatsByUserId(int userId)
    {
        return _context.Chats
            .Include(c => c.User1)
            .Include(c => c.User2)
            .Include(c => c.Messages)
            .Where(c => c.User1Id == userId || c.User2Id == userId)
            .OrderByDescending(c => c.CreatedDate)
            .ToList();
    }

    public Chat CreateChat(int user1Id, int user2Id)
    {
        var chat = new Chat
        {
            User1Id = user1Id,
            User2Id = user2Id,
            CreatedDate = DateTime.Now
        };

        _context.Chats.Add(chat);
        _context.SaveChanges();
        return chat;
    }

    public Chat UpdateChat(Chat chat)
    {
        _context.Chats.Update(chat);
        _context.SaveChanges();
        return chat;
    }

    public bool DeleteChat(int chatId)
    {
        var chat = _context.Chats.Find(chatId);
        if (chat != null)
        {
            _context.Chats.Remove(chat);
            _context.SaveChanges();
            return true;
        }
        return false;
    }

    public bool ChatExists(int chatId)
    {
        return _context.Chats.Any(c => c.ChatId == chatId);
    }

    public bool ChatExistsBetweenUsers(int user1Id, int user2Id)
    {
        return _context.Chats.Any(c => 
            (c.User1Id == user1Id && c.User2Id == user2Id) ||
            (c.User1Id == user2Id && c.User2Id == user1Id));
    }
}

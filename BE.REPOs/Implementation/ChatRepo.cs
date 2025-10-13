using BE.BOs.Models;
using BE.DAOs;
using BE.REPOs.Interface;

namespace BE.REPOs.Implementation;

public class ChatRepo : IChatRepo
{
    private readonly ChatDAO _chatDAO;

    public ChatRepo(ChatDAO chatDAO)
    {
        _chatDAO = chatDAO;
    }

    public List<Chat> GetAllChats()
    {
        return _chatDAO.GetAllChats();
    }

    public Chat? GetChatById(int chatId)
    {
        return _chatDAO.GetChatById(chatId);
    }

    public Chat? GetChatByUsers(int user1Id, int user2Id)
    {
        return _chatDAO.GetChatByUsers(user1Id, user2Id);
    }

    public List<Chat> GetChatsByUserId(int userId)
    {
        return _chatDAO.GetChatsByUserId(userId);
    }

    public Chat CreateChat(int user1Id, int user2Id)
    {
        return _chatDAO.CreateChat(user1Id, user2Id);
    }

    public Chat UpdateChat(Chat chat)
    {
        return _chatDAO.UpdateChat(chat);
    }

    public bool DeleteChat(int chatId)
    {
        return _chatDAO.DeleteChat(chatId);
    }

    public bool ChatExists(int chatId)
    {
        return _chatDAO.ChatExists(chatId);
    }

    public bool ChatExistsBetweenUsers(int user1Id, int user2Id)
    {
        return _chatDAO.ChatExistsBetweenUsers(user1Id, user2Id);
    }
}

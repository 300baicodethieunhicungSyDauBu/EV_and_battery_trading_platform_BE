using BE.BOs.Models;

namespace BE.REPOs.Interface;

public interface IChatRepo
{
    List<Chat> GetAllChats();
    Chat? GetChatById(int chatId);
    Chat? GetChatByUsers(int user1Id, int user2Id);
    List<Chat> GetChatsByUserId(int userId);
    Chat CreateChat(int user1Id, int user2Id);
    Chat UpdateChat(Chat chat);
    bool DeleteChat(int chatId);
    bool ChatExists(int chatId);
    bool ChatExistsBetweenUsers(int user1Id, int user2Id);
}

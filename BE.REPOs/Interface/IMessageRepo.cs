using BE.BOs.Models;

namespace BE.REPOs.Interface;

public interface IMessageRepo
{
    List<Message> GetAllMessages();
    Message? GetMessageById(int messageId);
    List<Message> GetMessagesByChatId(int chatId);
    List<Message> GetMessagesBySenderId(int senderId);
    List<Message> GetUnreadMessagesByUserId(int userId);
    Message CreateMessage(int chatId, int senderId, string content);
    Message UpdateMessage(Message message);
    bool DeleteMessage(int messageId);
    bool MarkMessageAsRead(int messageId);
    bool MarkMessagesAsReadByChatId(int chatId, int userId);
    bool MessageExists(int messageId);
    int GetUnreadMessageCount(int userId);
}

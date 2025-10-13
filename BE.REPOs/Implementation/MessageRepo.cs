using BE.BOs.Models;
using BE.DAOs;
using BE.REPOs.Interface;

namespace BE.REPOs.Implementation;

public class MessageRepo : IMessageRepo
{
    private readonly MessageDAO _messageDAO;

    public MessageRepo(MessageDAO messageDAO)
    {
        _messageDAO = messageDAO;
    }

    public List<Message> GetAllMessages()
    {
        return _messageDAO.GetAllMessages();
    }

    public Message? GetMessageById(int messageId)
    {
        return _messageDAO.GetMessageById(messageId);
    }

    public List<Message> GetMessagesByChatId(int chatId)
    {
        return _messageDAO.GetMessagesByChatId(chatId);
    }

    public List<Message> GetMessagesBySenderId(int senderId)
    {
        return _messageDAO.GetMessagesBySenderId(senderId);
    }

    public List<Message> GetUnreadMessagesByUserId(int userId)
    {
        return _messageDAO.GetUnreadMessagesByUserId(userId);
    }

    public Message CreateMessage(int chatId, int senderId, string content)
    {
        return _messageDAO.CreateMessage(chatId, senderId, content);
    }

    public Message UpdateMessage(Message message)
    {
        return _messageDAO.UpdateMessage(message);
    }

    public bool DeleteMessage(int messageId)
    {
        return _messageDAO.DeleteMessage(messageId);
    }

    public bool MarkMessageAsRead(int messageId)
    {
        return _messageDAO.MarkMessageAsRead(messageId);
    }

    public bool MarkMessagesAsReadByChatId(int chatId, int userId)
    {
        return _messageDAO.MarkMessagesAsReadByChatId(chatId, userId);
    }

    public bool MessageExists(int messageId)
    {
        return _messageDAO.MessageExists(messageId);
    }

    public int GetUnreadMessageCount(int userId)
    {
        return _messageDAO.GetUnreadMessageCount(userId);
    }
}

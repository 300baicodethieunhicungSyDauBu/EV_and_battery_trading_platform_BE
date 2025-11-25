using BE.BOs.Models;

namespace BE.REPOs.Interface
{
    public interface ICreditHistoryRepo
    {
        Task<CreditHistory> LogCreditChange(CreditHistory history);
        
        List<CreditHistory> GetUserCreditHistory(int userId);
        
        List<CreditHistory> GetAllCreditHistory();
        
        List<CreditHistory> GetCreditHistoryByType(string changeType);
        
        CreditHistory? GetHistoryById(int historyId);
    }
}

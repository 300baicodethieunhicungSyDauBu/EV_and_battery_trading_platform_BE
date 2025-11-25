using BE.BOs.Models;
using BE.DAOs;
using BE.REPOs.Interface;

namespace BE.REPOs.Implementation
{
    public class CreditHistoryRepo : ICreditHistoryRepo
    {
        public async Task<CreditHistory> LogCreditChange(CreditHistory history)
        {
            return await CreditHistoryDAO.Instance.LogCreditChange(history);
        }

        public List<CreditHistory> GetUserCreditHistory(int userId)
        {
            return CreditHistoryDAO.Instance.GetUserCreditHistory(userId);
        }

        public List<CreditHistory> GetAllCreditHistory()
        {
            return CreditHistoryDAO.Instance.GetAllCreditHistory();
        }

        public List<CreditHistory> GetCreditHistoryByType(string changeType)
        {
            return CreditHistoryDAO.Instance.GetCreditHistoryByType(changeType);
        }

        public CreditHistory? GetHistoryById(int historyId)
        {
            return CreditHistoryDAO.Instance.GetHistoryById(historyId);
        }
    }
}

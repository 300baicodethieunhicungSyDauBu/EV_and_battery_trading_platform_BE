using BE.BOs.Models;
using Microsoft.EntityFrameworkCore;

namespace BE.DAOs
{
    public class CreditHistoryDAO
    {
        private static CreditHistoryDAO? _instance;
        private static readonly object _lock = new object();

        public static CreditHistoryDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new CreditHistoryDAO();
                        }
                    }
                }
                return _instance;
            }
        }

        public async Task<CreditHistory> LogCreditChange(CreditHistory history)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            
            history.CreatedDate = DateTime.Now;
            context.CreditHistories.Add(history);
            await context.SaveChangesAsync();
            
            return history;
        }

        public List<CreditHistory> GetUserCreditHistory(int userId)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            
            return context.CreditHistories
                .Include(h => h.User)
                .Include(h => h.Payment)
                .Include(h => h.Product)
                .Include(h => h.CreatedByUser)
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.CreatedDate)
                .ToList();
        }

        public List<CreditHistory> GetAllCreditHistory()
        {
            using var context = new EvandBatteryTradingPlatformContext();
            
            return context.CreditHistories
                .Include(h => h.User)
                .Include(h => h.Payment)
                .Include(h => h.Product)
                .Include(h => h.CreatedByUser)
                .OrderByDescending(h => h.CreatedDate)
                .ToList();
        }

        public List<CreditHistory> GetCreditHistoryByType(string changeType)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            
            return context.CreditHistories
                .Include(h => h.User)
                .Include(h => h.Payment)
                .Include(h => h.Product)
                .Include(h => h.CreatedByUser)
                .Where(h => h.ChangeType == changeType)
                .OrderByDescending(h => h.CreatedDate)
                .ToList();
        }

        public CreditHistory? GetHistoryById(int historyId)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            
            return context.CreditHistories
                .Include(h => h.User)
                .Include(h => h.Payment)
                .Include(h => h.Product)
                .Include(h => h.CreatedByUser)
                .FirstOrDefault(h => h.HistoryId == historyId);
        }
    }
}

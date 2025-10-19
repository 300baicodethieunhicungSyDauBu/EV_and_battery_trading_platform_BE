using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BE.BOs.Models; // Payment, DbContext
using BE.REPOs.Interface;
using Microsoft.EntityFrameworkCore;

namespace BE.REPOs.Implementation
{
    public class PaymentRepo : IPaymentRepo
    {
        private readonly EvandBatteryTradingPlatformContext _db;
        public PaymentRepo(EvandBatteryTradingPlatformContext db) => _db = db;

        // CRUD cơ bản (async)
        public async Task<IReadOnlyList<Payment>> GetAllPaymentsAsync()
            => await _db.Payments.AsNoTracking().OrderByDescending(p => p.CreatedDate).ToListAsync();

        public async Task<Payment?> GetPaymentByIdAsync(int id)
            => await _db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.PaymentId == id);

        public async Task<Payment> CreatePaymentAsync(Payment payment)
        {
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();
            return payment;
        }

        public async Task<Payment> UpdatePaymentAsync(Payment payment)
        {
            _db.Payments.Update(payment);
            await _db.SaveChangesAsync();
            return payment;
        }

        // CRUD cơ bản (sync) - để tương thích với controller hiện tại
        public IReadOnlyList<Payment> GetAllPayments()
            => _db.Payments.AsNoTracking().OrderByDescending(p => p.CreatedDate).ToList();

        public Payment? GetPaymentById(int id)
            => _db.Payments.AsNoTracking().FirstOrDefault(p => p.PaymentId == id);

        public Payment CreatePayment(Payment payment)
        {
            _db.Payments.Add(payment);
            _db.SaveChanges();
            return payment;
        }

        public Payment UpdatePayment(Payment payment)
        {
            _db.Payments.Update(payment);
            _db.SaveChanges();
            return payment;
        }

        // Tra cứu phục vụ nghiệp vụ (async)
        public async Task<IReadOnlyList<Payment>> GetPaymentsByOrderIdAsync(int orderId)
            => await _db.Payments.AsNoTracking().Where(p => p.OrderId == orderId).ToListAsync();

        public async Task<IReadOnlyList<Payment>> GetPaymentsByPayerIdAsync(int payerId)
            => await _db.Payments.AsNoTracking().Where(p => p.PayerId == payerId).ToListAsync();

        // Tra cứu phục vụ nghiệp vụ (sync) - để tương thích với controller hiện tại
        public IReadOnlyList<Payment> GetPaymentsByOrderId(int orderId)
            => _db.Payments.AsNoTracking().Where(p => p.OrderId == orderId).ToList();

        public IReadOnlyList<Payment> GetPaymentsByPayerId(int payerId)
            => _db.Payments.AsNoTracking().Where(p => p.PayerId == payerId).ToList();

        // VNPay helpers

        // Lấy bản ghi để update an toàn (tracking bật)
        public async Task<Payment?> GetPaymentForUpdateAsync(int id)
            => await _db.Payments.FirstOrDefaultAsync(p => p.PaymentId == id);

        // Nếu sau này TxnRef != PaymentId, bạn có thể lưu riêng; tạm thời mapping PaymentId
        public async Task<Payment?> GetByTxnRefAsync(string txnRef)
        {
            if (int.TryParse(txnRef, out var pid))
                return await _db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.PaymentId == pid);
            return null;
        }

        public async Task<Payment?> GetByTransactionNoAsync(string transactionNo)
            => await _db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.TransactionNo == transactionNo);

        public async Task<bool> HasSuccessfulPaymentAsync(int paymentId)
            => await _db.Payments.AsNoTracking().AnyAsync(p => p.PaymentId == paymentId && p.Status == "Success");

        public async Task<bool> HasSuccessfulPaymentForOrderAsync(int orderId, string paymentType)
            => await _db.Payments.AsNoTracking().AnyAsync(p => p.OrderId == orderId
                                                            && p.PaymentType == paymentType
                                                            && p.Status == "Success");

        // Chưa có bảng audit => tạm no-op để compile
        public Task AddPaymentAuditAsync(int paymentId, string channel, string rawQuery, string? note = null)
            => Task.CompletedTask;
    }
}

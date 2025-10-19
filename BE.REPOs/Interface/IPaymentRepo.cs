using BE.BOs.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BE.REPOs.Interface
{
    public interface IPaymentRepo
    {
        // CRUD cơ bản (async)
        Task<IReadOnlyList<Payment>> GetAllPaymentsAsync();
        Task<Payment?> GetPaymentByIdAsync(int id);
        Task<Payment> CreatePaymentAsync(Payment payment);
        Task<Payment> UpdatePaymentAsync(Payment payment);

        // CRUD cơ bản (sync) - để tương thích với controller hiện tại
        IReadOnlyList<Payment> GetAllPayments();
        Payment? GetPaymentById(int id);
        Payment CreatePayment(Payment payment);
        Payment UpdatePayment(Payment payment);

        // Tra cứu phục vụ nghiệp vụ (async)
        Task<IReadOnlyList<Payment>> GetPaymentsByOrderIdAsync(int orderId);
        Task<IReadOnlyList<Payment>> GetPaymentsByPayerIdAsync(int payerId);

        // Tra cứu phục vụ nghiệp vụ (sync) - để tương thích với controller hiện tại
        IReadOnlyList<Payment> GetPaymentsByOrderId(int orderId);
        IReadOnlyList<Payment> GetPaymentsByPayerId(int payerId);

        // ★ Thêm cho VNPay:
        // Lấy bản ghi để update an toàn (tuỳ ORM bạn có thể dùng rowversion/concurrency token)
        Task<Payment?> GetPaymentForUpdateAsync(int id);

        // Nếu sau này TxnRef != PaymentId, có thể cần:
        Task<Payment?> GetByTxnRefAsync(string txnRef);
        Task<Payment?> GetByTransactionNoAsync(string transactionNo);

        // Kiểm tra idempotency
        Task<bool> HasSuccessfulPaymentAsync(int paymentId);
        Task<bool> HasSuccessfulPaymentForOrderAsync(int orderId, string paymentType);

        // (Khuyến nghị) Log đối soát VNPay
        Task AddPaymentAuditAsync(int paymentId, string channel /*Return/IPN*/, string rawQuery, string? note = null);
    }
}

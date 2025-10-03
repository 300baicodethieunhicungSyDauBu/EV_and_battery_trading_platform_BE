using BE.BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.REPOs.Interface
{
    public interface IPaymentRepo
    {
        List<Payment> GetAllPayments();
        Payment GetPaymentById(int id);
        Payment CreatePayment(Payment payment);
        Payment UpdatePayment(Payment payment);
        List<Payment> GetPaymentsByOrderId(int orderId);
        List<Payment> GetPaymentsByPayerId(int payerId);
    }
}

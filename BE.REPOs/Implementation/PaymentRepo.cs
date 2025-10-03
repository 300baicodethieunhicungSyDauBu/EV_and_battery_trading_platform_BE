using BE.BOs.Models;
using BE.DAOs;
using BE.REPOs.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.REPOs.Implementation
{
    public class PaymentRepo : IPaymentRepo
    {
        public List<Payment> GetAllPayments()
        {
            return PaymentDAO.Instance.GetAllPayments();
        }

        public Payment GetPaymentById(int id)
        {
            return PaymentDAO.Instance.GetPaymentById(id);
        }

        public Payment CreatePayment(Payment payment)
        {
            return PaymentDAO.Instance.CreatePayment(payment);
        }

        public Payment UpdatePayment(Payment payment)
        {
            return PaymentDAO.Instance.UpdatePayment(payment);
        }

        public List<Payment> GetPaymentsByOrderId(int orderId)
        {
            return PaymentDAO.Instance.GetPaymentsByOrderId(orderId);
        }

        public List<Payment> GetPaymentsByPayerId(int payerId)
        {
            return PaymentDAO.Instance.GetPaymentsByPayerId(payerId);
        }
    }
}

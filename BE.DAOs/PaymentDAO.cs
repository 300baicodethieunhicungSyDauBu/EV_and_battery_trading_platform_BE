using BE.BOs.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.DAOs
{
    public class PaymentDAO
    {
        private static PaymentDAO? instance;
        private static EvandBatteryTradingPlatformContext? dbcontext;

        private PaymentDAO()
        {
            dbcontext = new EvandBatteryTradingPlatformContext();
        }

        public static PaymentDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PaymentDAO();
                }
                return instance;
            }
        }

        public List<Payment> GetAllPayments()
        {
            return dbcontext.Payments
                .Include(p => p.Order)
                .Include(p => p.Payer)
                .ToList();
        }

        public Payment? GetPaymentById(int id)
        {
            return dbcontext.Payments
                .Include(p => p.Order)
                .Include(p => p.Payer)
                .FirstOrDefault(p => p.PaymentId == id);
        }

        public Payment CreatePayment(Payment payment)
        {
            payment.CreatedDate = DateTime.Now;
            payment.Status = "Pending";
            dbcontext.Payments.Add(payment);
            dbcontext.SaveChanges();
            return payment;
        }

        public Payment UpdatePayment(Payment payment)
        {
            dbcontext.Payments.Update(payment);
            dbcontext.SaveChanges();
            return payment;
        }

        public List<Payment> GetPaymentsByOrderId(int orderId)
        {
            return dbcontext.Payments
                .Include(p => p.Order)
                .Include(p => p.Payer)
                .Where(p => p.OrderId == orderId)
                .ToList();
        }

        public List<Payment> GetPaymentsByPayerId(int payerId)
        {
            return dbcontext.Payments
                .Include(p => p.Order)
                .Include(p => p.Payer)
                .Where(p => p.PayerId == payerId)
                .ToList();
        }
    }
}


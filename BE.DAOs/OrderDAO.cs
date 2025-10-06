using BE.BOs.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.DAOs
{
    public class OrderDAO
    {
        private static OrderDAO? instance;
        private static EvandBatteryTradingPlatformContext? dbcontext;

        private OrderDAO()
        {
            dbcontext = new EvandBatteryTradingPlatformContext();
        }

        public static OrderDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new OrderDAO();
                }
                return instance;
            }
        }

        public List<Order> GetAllOrders()
        {
            return dbcontext.Orders
                .Include(o => o.Buyer)
                .Include(o => o.Seller)
                .Include(o => o.Product)
                .Include(o => o.Payments)
                .ToList();
        }

        public Order? GetOrderById(int id)
        {
            return dbcontext.Orders
                .Include(o => o.Buyer)
                .Include(o => o.Seller)
                .Include(o => o.Product)
                .Include(o => o.Payments)
                .FirstOrDefault(o => o.OrderId == id);
        }

        public Order CreateOrder(Order order)
        {
            order.CreatedDate = DateTime.Now;
            order.Status = "Pending";
            order.DepositStatus = "Pending";
            order.FinalPaymentStatus = "Pending";
            order.PayoutStatus = "Pending";
            dbcontext.Orders.Add(order);
            dbcontext.SaveChanges();
            return order;
        }

        public Order UpdateOrder(Order order)
        {
            dbcontext.Orders.Update(order);
            dbcontext.SaveChanges();
            return order;
        }

        public List<Order> GetOrdersByBuyerId(int buyerId)
        {
            return dbcontext.Orders
                .Include(o => o.Seller)
                .Include(o => o.Product)
                .Include(o => o.Payments)
                .Where(o => o.BuyerId == buyerId)
                .ToList();
        }

        public List<Order> GetOrdersBySellerId(int sellerId)
        {
            return dbcontext.Orders
                .Include(o => o.Buyer)
                .Include(o => o.Product)
                .Include(o => o.Payments)
                .Where(o => o.SellerId == sellerId)
                .ToList();
        }
    }
}


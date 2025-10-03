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
    public class OrderRepo : IOrderRepo
    {
        public List<Order> GetAllOrders()
        {
            return OrderDAO.Instance.GetAllOrders();
        }

        public Order GetOrderById(int id)
        {
            return OrderDAO.Instance.GetOrderById(id);
        }

        public Order CreateOrder(Order order)
        {
            return OrderDAO.Instance.CreateOrder(order);
        }

        public Order UpdateOrder(Order order)
        {
            return OrderDAO.Instance.UpdateOrder(order);
        }

        public List<Order> GetOrdersByBuyerId(int buyerId)
        {
            return OrderDAO.Instance.GetOrdersByBuyerId(buyerId);
        }

        public List<Order> GetOrdersBySellerId(int sellerId)
        {
            return OrderDAO.Instance.GetOrdersBySellerId(sellerId);
        }
    }
}

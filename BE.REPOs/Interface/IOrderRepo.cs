using BE.BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.REPOs.Interface
{
    public interface IOrderRepo
    {
        List<Order> GetAllOrders();
        Order GetOrderById(int id);
        Order CreateOrder(Order order);
        Order UpdateOrder(Order order);
        List<Order> GetOrdersByBuyerId(int buyerId);
        List<Order> GetOrdersBySellerId(int sellerId);
    }
}

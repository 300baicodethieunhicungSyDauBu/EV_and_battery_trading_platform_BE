using BE.BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.REPOs.Interface
{
    public interface IProductRepo
    {
        List<Product> GetAllProducts();
        Product GetProductById(int id);
        Product CreateProduct(Product product);
        Product UpdateProduct(Product product);
        bool DeleteProduct(int id);
        List<Product> GetProductsBySellerId(int sellerId);
        List<Product> GetDraftProducts();
        Product ApproveProduct(int id);
        List<Product> GetActiveProducts();
    }
}

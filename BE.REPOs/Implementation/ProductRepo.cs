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
    public class ProductRepo : IProductRepo
    {
        public List<Product> GetAllProducts()
        {
            return ProductDAO.Instance.GetAllProducts();
        }

        public Product GetProductById(int id)
        {
            return ProductDAO.Instance.GetProductById(id);
        }

        public Product CreateProduct(Product product)
        {
            return ProductDAO.Instance.CreateProduct(product);
        }

        public Product UpdateProduct(Product product)
        {
            return ProductDAO.Instance.UpdateProduct(product);
        }

        public bool DeleteProduct(int id)
        {
            return ProductDAO.Instance.DeleteProduct(id);
        }

        public List<Product> GetProductsBySellerId(int sellerId)
        {
            return ProductDAO.Instance.GetProductsBySellerId(sellerId);
        }

        public List<Product> GetDraftProducts()
        {
            return ProductDAO.Instance.GetDraftProducts();
        }

        public Product ApproveProduct(int id)
        {
            return ProductDAO.Instance.ApproveProduct(id);
        }

        public Product RejectProduct(int id, string? rejectionReason = null)
        {
            return ProductDAO.Instance.RejectProduct(id, rejectionReason);
        }

        public List<Product> GetActiveProducts()
        {
            return ProductDAO.Instance.GetAciveProducts();
        }

        public List<Product> GetProductsByLicensePlate(string licensePlate)
        {
            return ProductDAO.Instance.GetProductsByLicensePlate(licensePlate);
        }

        public Product GetProductByExactLicensePlate(string licensePlate)
        {
            return ProductDAO.Instance.GetProductByExactLicensePlate(licensePlate);
        }

        public List<Product> GetProductsByType(string productType)
        {
            return ProductDAO.Instance.GetProductsByType(productType);
        }
        
        public List<Product> GetReSubmittedProducts()
        {
            return ProductDAO.Instance.GetReSubmittedProducts();
        }
    }
}

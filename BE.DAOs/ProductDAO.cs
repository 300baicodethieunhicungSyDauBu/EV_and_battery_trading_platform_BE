using BE.BOs.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.DAOs
{
    public class ProductDAO
    {
        private static ProductDAO? instance;
        private static EvandBatteryTradingPlatformContext? dbcontext;

        private ProductDAO()
        {
            dbcontext = new EvandBatteryTradingPlatformContext();
        }

        public static ProductDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ProductDAO();
                }
                return instance;
            }
        }

        public List<Product> GetAllProducts()
        {
            return dbcontext?.Products
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .ToList() ?? new List<Product>();
        }

        public Product? GetProductById(int id)
        {
            return dbcontext.Products
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.ProductId == id);
        }

        public Product CreateProduct(Product product)
        {
            product.CreatedDate = DateTime.Now;
            product.Status = "Draft";
            product.VerificationStatus = "NotRequested";
            dbcontext.Products.Add(product);
            dbcontext.SaveChanges();
            return product;
        }

        public Product UpdateProduct(Product product)
        {
            dbcontext.Products.Update(product);
            dbcontext.SaveChanges();
            return product;
        }

        public bool DeleteProduct(int id)
        {
            var product = dbcontext.Products.Find(id);
            if (product == null) return false;

            product.Status = "Deleted";
            dbcontext.SaveChanges();
            return true;
        }

        public List<Product> GetProductsBySellerId(int sellerId)
        {
            return dbcontext.Products
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .Where(p => p.SellerId == sellerId)
                .ToList();
        }

        public List<Product> GetDraftProducts()
        {
            return dbcontext.Products
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .Where(p => p.Status == "Draft")
                .ToList();
        }

        public Product? ApproveProduct(int id)
        {
            var product = dbcontext.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null) return null;

            product.Status = "Active";
            product.VerificationStatus = "Approved";
            dbcontext.SaveChanges();
            return product;
        }

        public Product? RejectProduct(int id, string? rejectionReason = null)
        {
            var product = dbcontext.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null) return null;

            product.Status = "Rejected";
            product.VerificationStatus = "Rejected";
            product.RejectionReason = rejectionReason;
            dbcontext.SaveChanges();
            return product;
        }

        public List<Product> GetAciveProducts()
        {
            return dbcontext.Products
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .Where(p => p.Status == "Active")
                .ToList();
        }

        public List<Product> GetProductsByLicensePlate(string licensePlate)
        {
            return dbcontext.Products
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .Where(p => p.LicensePlate != null && p.LicensePlate.Contains(licensePlate))
                .ToList();
        }

        public Product? GetProductByExactLicensePlate(string licensePlate)
        {
            return dbcontext.Products
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.LicensePlate == licensePlate);
        }

        public List<Product> GetProductsByType(string productType)
        {
            return dbcontext.Products
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .Where(p => p.ProductType != null && p.ProductType.ToLower().Contains(productType.ToLower()) && p.Status == "Active")
                .ToList();
        }
        
        public List<Product> GetReSubmittedProducts()
        {
            return dbcontext.Products
                .Include(p => p.Seller)
                .Include(p => p.ProductImages)
                .Where(p => p.Status == "Re-submit")
                .ToList();
        }
    }
}


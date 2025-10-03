using BE.BOs.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.DAOs
{
    public class ProductImageDAO
    {
        private static ProductImageDAO instance;
        private static EvandBatteryTradingPlatformContext dbcontext;

        private ProductImageDAO()
        {
            dbcontext = new EvandBatteryTradingPlatformContext();
        }

        public static ProductImageDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ProductImageDAO();
                }
                return instance;
            }
        }

        public List<ProductImage> GetAllProductImages()
        {
            return dbcontext.ProductImages
                .Include(pi => pi.Product)
                .ToList();
        }

        public List<ProductImage> GetImagesByProductId(int productId)
        {
            return dbcontext.ProductImages
                .Where(pi => pi.ProductId == productId)
                .ToList();
        }

        public ProductImage GetProductImageById(int id)
        {
            return dbcontext.ProductImages
                .Include(pi => pi.Product)
                .FirstOrDefault(pi => pi.ImageId == id);
        }

        public ProductImage CreateProductImage(ProductImage productImage)
        {
            dbcontext.ProductImages.Add(productImage);
            dbcontext.SaveChanges();
            return productImage;
        }

        public ProductImage UpdateProductImage(ProductImage productImage)
        {
            dbcontext.ProductImages.Update(productImage);
            dbcontext.SaveChanges();
            return productImage;
        }

        public bool DeleteProductImage(int id)
        {
            var productImage = dbcontext.ProductImages.Find(id);
            if (productImage == null) return false;

            dbcontext.ProductImages.Remove(productImage);
            dbcontext.SaveChanges();
            return true;
        }
    }
}

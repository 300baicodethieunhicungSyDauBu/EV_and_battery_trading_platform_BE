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
    public class ProductImageRepo : IProductImageRepo
    {
        public List<ProductImage> GetAllProductImages()
        {
            return ProductImageDAO.Instance.GetAllProductImages();
        }

        public List<ProductImage> GetImagesByProductId(int productId)
        {
            return ProductImageDAO.Instance.GetImagesByProductId(productId);
        }

        public ProductImage GetProductImageById(int id)
        {
            return ProductImageDAO.Instance.GetProductImageById(id);
        }

        public ProductImage CreateProductImage(ProductImage productImage)
        {
            return ProductImageDAO.Instance.CreateProductImage(productImage);
        }

        public ProductImage UpdateProductImage(ProductImage productImage)
        {
            return ProductImageDAO.Instance.UpdateProductImage(productImage);
        }

        public bool DeleteProductImage(int id)
        {
            return ProductImageDAO.Instance.DeleteProductImage(id);
        }
    }
}

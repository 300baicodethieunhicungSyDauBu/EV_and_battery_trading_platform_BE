using BE.BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.REPOs.Interface
{
    public interface IProductImageRepo
    {
        List<ProductImage> GetAllProductImages();
        List<ProductImage> GetImagesByProductId(int productId);
        ProductImage GetProductImageById(int id);
        ProductImage CreateProductImage(ProductImage productImage);
        ProductImage UpdateProductImage(ProductImage productImage);
        bool DeleteProductImage(int id);
    }
}

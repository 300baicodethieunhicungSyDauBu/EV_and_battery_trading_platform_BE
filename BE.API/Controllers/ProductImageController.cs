using BE.API.DTOs.Request;
using BE.API.DTOs.Response;
using BE.BOs.Models;
using BE.REPOs.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductImageController : ControllerBase
    {
        private readonly IProductImageRepo _productImageRepo;
        private readonly IProductRepo _productRepo;

        public ProductImageController(IProductImageRepo productImageRepo, IProductRepo productRepo)
        {
            _productImageRepo = productImageRepo;
            _productRepo = productRepo;
        }

        [HttpGet("product/{productId}")]
        public ActionResult<IEnumerable<ProductImageResponse>> GetProductImages(int productId)
        {
            try
            {
                var images = _productImageRepo.GetImagesByProductId(productId);
                var response = images.Select(img => new ProductImageResponse
                {
                    ImageId = img.ImageId,
                    ProductId = img.ProductId,
                    ImageData = img.ImageData,
                    CreatedDate = img.CreatedDate
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("{id}")]
        public ActionResult<ProductImageResponse> GetProductImage(int id)
        {
            try
            {
                var image = _productImageRepo.GetProductImageById(id);
                if (image == null)
                {
                    return NotFound();
                }

                var response = new ProductImageResponse
                {
                    ImageId = image.ImageId,
                    ProductId = image.ProductId,
                    ImageData = image.ImageData,
                    CreatedDate = image.CreatedDate
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Policy = "MemberOnly")]
        public ActionResult<ProductImageResponse> CreateProductImage([FromBody] ProductImageRequest request)
        {
            try
            {
                // Verify product ownership
                var product = _productRepo.GetProductById(request.ProductId ?? 0);
                if (product == null)
                {
                    return NotFound("Product not found");
                }

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (product.SellerId != userId && !User.IsInRole("1"))
                {
                    return Forbid();
                }

                var productImage = new ProductImage
                {
                    ProductId = request.ProductId,
                    ImageData = request.ImageData,
                    CreatedDate = DateTime.Now
                };

                var createdImage = _productImageRepo.CreateProductImage(productImage);

                var response = new ProductImageResponse
                {
                    ImageId = createdImage.ImageId,
                    ProductId = createdImage.ProductId,
                    ImageData = createdImage.ImageData,
                    CreatedDate = createdImage.CreatedDate
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "MemberOnly")]
        public ActionResult<ProductImageResponse> UpdateProductImage(int id, [FromBody] ProductImageRequest request)
        {
            try
            {
                var existingImage = _productImageRepo.GetProductImageById(id);
                if (existingImage == null)
                {
                    return NotFound();
                }

                // Verify product ownership
                var product = _productRepo.GetProductById(existingImage.ProductId ?? 0);
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (product?.SellerId != userId && !User.IsInRole("1"))
                {
                    return Forbid();
                }

                existingImage.ImageData = request.ImageData;

                var updatedImage = _productImageRepo.UpdateProductImage(existingImage);

                var response = new ProductImageResponse
                {
                    ImageId = updatedImage.ImageId,
                    ProductId = updatedImage.ProductId,
                    ImageData = updatedImage.ImageData,
                    CreatedDate = updatedImage.CreatedDate
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "MemberOnly")]
        public ActionResult DeleteProductImage(int id)
        {
            try
            {
                var image = _productImageRepo.GetProductImageById(id);
                if (image == null)
                {
                    return NotFound();
                }

                // Verify product ownership
                var product = _productRepo.GetProductById(image.ProductId ?? 0);
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (product?.SellerId != userId && !User.IsInRole("1"))
                {
                    return Forbid();
                }

                var result = _productImageRepo.DeleteProductImage(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }

}

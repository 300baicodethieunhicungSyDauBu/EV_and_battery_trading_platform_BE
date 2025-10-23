using BE.API.DTOs.Request;
using BE.API.DTOs.Response;
using BE.BOs.Models;
using BE.REPOs.Interface;
using BE.REPOs.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductImageController : ControllerBase
    {
        private readonly IProductImageRepo _productImageRepo;
        private readonly IProductRepo _productRepo;
        private readonly CloudinaryService _cloudinaryService;

        public ProductImageController(IProductImageRepo productImageRepo, IProductRepo productRepo,
            CloudinaryService cloudinaryService)
        {
            _productImageRepo = productImageRepo;
            _productRepo = productRepo;
            _cloudinaryService = cloudinaryService;
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
                    Name = img.Name,
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
                    Name = image.Name,
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
        public async Task<ActionResult<ProductImageResponse>> CreateProductImage([FromForm] ProductImageRequest request)
        {
            try
            {
                // ✅ Kiểm tra sản phẩm có tồn tại không
                var product = _productRepo.GetProductById(request.ProductId ?? 0);
                if (product == null)
                    return NotFound("Product not found");

                // ✅ Kiểm tra quyền sở hữu sản phẩm
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (product.SellerId != userId && !User.IsInRole("1")) // "1" = Admin
                    return Forbid();

                // ✅ Kiểm tra tên loại ảnh
                if (string.IsNullOrWhiteSpace(request.Name) ||
                    !new[] { "Vehicle", "Battery", "Document" }.Contains(request.Name,
                        StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest("Name must be one of: Vehicle, Battery, or Document.");
                }

                // ✅ Upload ảnh lên Cloudinary
                string imageUrl = await _cloudinaryService.UploadImageAsync(request.ImageFile);

                // ✅ Tạo record ProductImage mới
                var productImage = new ProductImage
                {
                    ProductId = request.ProductId,
                    ImageData = imageUrl,
                    Name = request.Name, // 🟢 Thêm tên loại ảnh
                    CreatedDate = DateTime.Now
                };

                var createdImage = _productImageRepo.CreateProductImage(productImage);

                // ✅ Tạo response trả về
                var response = new ProductImageResponse
                {
                    ImageId = createdImage.ImageId,
                    ProductId = createdImage.ProductId,
                    Name = createdImage.Name,
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

        [HttpPost("multiple")]
        public async Task<ActionResult<List<ProductImageResponse>>> CreateMultipleProductImages(
            [FromForm] int productId,
            [FromForm] string name,
            [FromForm] List<IFormFile> images)
        {
            try
            {
                // ✅ Kiểm tra sản phẩm tồn tại
                var product = _productRepo.GetProductById(productId);
                if (product == null)
                    return NotFound("Product not found");

                // ✅ Kiểm tra quyền sở hữu
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (product.SellerId != userId && !User.IsInRole("1")) // "1" = Admin
                    return Forbid();

                // ✅ Validate loại ảnh (Vehicle/Battery/Document)
                if (string.IsNullOrWhiteSpace(name) ||
                    !new[] { "Vehicle", "Battery", "Document" }.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest("Name must be one of: Vehicle, Battery, or Document.");
                }

                // ✅ Validate danh sách ảnh
                if (images == null || !images.Any())
                    return BadRequest("At least one image is required.");

                var responses = new List<ProductImageResponse>();

                foreach (var image in images)
                {
                    string imageUrl = await _cloudinaryService.UploadImageAsync(image);

                    var productImage = new ProductImage
                    {
                        ProductId = productId,
                        Name = name, // 🟢 Thêm tên loại ảnh
                        ImageData = imageUrl,
                        CreatedDate = DateTime.Now
                    };

                    var createdImage = _productImageRepo.CreateProductImage(productImage);

                    responses.Add(new ProductImageResponse
                    {
                        ImageId = createdImage.ImageId,
                        ProductId = createdImage.ProductId,
                        Name = createdImage.Name,
                        ImageData = createdImage.ImageData,
                        CreatedDate = createdImage.CreatedDate
                    });
                }

                return Ok(responses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductImageResponse>> UpdateProductImage(
            int id,
            [FromForm] ProductImageRequest request)
        {
            try
            {
                var existingImage = _productImageRepo.GetProductImageById(id);
                if (existingImage == null)
                {
                    return NotFound("Product image not found.");
                }

                // ✅ Kiểm tra quyền sở hữu sản phẩm
                var product = _productRepo.GetProductById(existingImage.ProductId ?? 0);
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

                if (product?.SellerId != userId && !User.IsInRole("1")) // 1 = Admin
                {
                    return Forbid("You are not authorized to modify this product image.");
                }

                // ✅ Upload ảnh mới lên Cloudinary nếu có
                string? newImageUrl = null;
                if (request.ImageFile != null)
                {
                    newImageUrl = await _cloudinaryService.UploadImageAsync(request.ImageFile);
                    existingImage.ImageData = newImageUrl;
                }

                // ✅ Cập nhật thời gian chỉnh sửa
                existingImage.CreatedDate = DateTime.Now;

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
        
        [HttpGet("product/{productId}/by-name")]
        public ActionResult<IEnumerable<ProductImageResponse>> GetProductImagesByName(int productId, [FromQuery] string name)
        {
            try
            {
                // ✅ Validate input
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest("Image name (Vehicle, Battery, or Document) is required.");

                // ✅ Validate name
                var allowedNames = new[] { "Vehicle", "Battery", "Document" };
                if (!allowedNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                    return BadRequest("Invalid name. Must be one of: Vehicle, Battery, Document.");

                // ✅ Kiểm tra sản phẩm tồn tại
                var product = _productRepo.GetProductById(productId);
                if (product == null)
                    return NotFound("Product not found.");

                // ✅ Lấy danh sách ảnh theo ProductId + Name
                var images = _productImageRepo
                    .GetImagesByProductId(productId)
                    .Where(i => i.Name != null && i.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!images.Any())
                    return NotFound($"No images found for product {productId} with name '{name}'.");

                // ✅ Map sang DTO response
                var response = images.Select(img => new ProductImageResponse
                {
                    ImageId = img.ImageId,
                    ProductId = img.ProductId,
                    Name = img.Name,
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
    }
}
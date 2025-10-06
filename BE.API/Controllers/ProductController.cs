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
    public class ProductController : ControllerBase
    {
        private readonly IProductRepo _productRepo;

        public ProductController(IProductRepo productRepo)
        {
            _productRepo = productRepo;
        }

        [HttpGet]
        public ActionResult GetAllProducts()
        {
            try
            {
                var products = _productRepo.GetAllProducts();
                var response = products.Select(p => new ProductResponse
                {
                    ProductId = p.ProductId,
                    SellerId = p.SellerId,
                    ProductType = p.ProductType,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    Brand = p.Brand,
                    Model = p.Model,
                    Condition = p.Condition,
                    VehicleType = p.VehicleType,
                    ManufactureYear = p.ManufactureYear,
                    Mileage = p.Mileage,
                    BatteryHealth = p.BatteryHealth,
                    BatteryType = p.BatteryType,
                    Capacity = p.Capacity,
                    Voltage = p.Voltage,
                    CycleCount = p.CycleCount,
                    LicensePlate = p.LicensePlate,
                    Status = p.Status,
                    VerificationStatus = p.VerificationStatus,
                    CreatedDate = p.CreatedDate,
                    ImageUrls = p.ProductImages.Select(img => img.ImageData).ToList() // ✅ map hình ảnh
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("{id}")]
        public ActionResult GetProductById(int id)
        {
            try
            {
                var product = _productRepo.GetProductById(id);
                if (product == null)
                    return NotFound();

                var response = new ProductResponse
                {
                    ProductId = product.ProductId,
                    SellerId = product.SellerId,
                    ProductType = product.ProductType,
                    Title = product.Title,
                    Description = product.Description,
                    Price = product.Price,
                    Brand = product.Brand,
                    Model = product.Model,
                    Condition = product.Condition,
                    VehicleType = product.VehicleType,
                    ManufactureYear = product.ManufactureYear,
                    Mileage = product.Mileage,
                    BatteryHealth = product.BatteryHealth,
                    BatteryType = product.BatteryType,
                    Capacity = product.Capacity,
                    Voltage = product.Voltage,
                    CycleCount = product.CycleCount,
                    LicensePlate = product.LicensePlate,
                    Status = product.Status,
                    VerificationStatus = product.VerificationStatus,
                    CreatedDate = product.CreatedDate,
                    ImageUrls = product.ProductImages.Select(img => img.ImageData).ToList() // ✅
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost]
        //[Authorize(Policy = "MemberOnly")]
        public ActionResult CreateProduct([FromBody] ProductRequest request)
        {
            try
            {
                // Validate license plate format for vehicles
                if (!string.IsNullOrEmpty(request.LicensePlate) && 
                    (request.ProductType?.ToLower().Contains("vehicle") == true || 
                     request.ProductType?.ToLower().Contains("xe") == true))
                {
                    if (!IsValidLicensePlate(request.LicensePlate))
                    {
                        return BadRequest("Invalid license plate format. Please use Vietnamese license plate format (e.g., 30A-12345, 51G-12345)");
                    }
                }

                var product = new Product
                {
                    SellerId = int.TryParse(User.FindFirst("UserId")?.Value, out var userId) ? userId : 0,
                    ProductType = request.ProductType,
                    Title = request.Title,
                    Description = request.Description,
                    Price = request.Price,
                    Brand = request.Brand,
                    Model = request.Model,
                    Condition = request.Condition,
                    VehicleType = request.VehicleType,
                    ManufactureYear = request.ManufactureYear,
                    Mileage = request.Mileage,
                    BatteryHealth = request.BatteryHealth,
                    BatteryType = request.BatteryType,
                    Capacity = request.Capacity,
                    Voltage = request.Voltage,
                    CycleCount = request.CycleCount,
                    LicensePlate = request.LicensePlate
                };

                var createdProduct = _productRepo.CreateProduct(product);

                var response = new
                {
                    createdProduct.ProductId,
                    createdProduct.Title,
                    createdProduct.Price,
                    createdProduct.Status,
                    createdProduct.VerificationStatus,
                    createdProduct.CreatedDate
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        //[Authorize(Policy = "MemberOnly")]
        public ActionResult UpdateProduct(int id, [FromBody] ProductRequest request)
        {
            try
            {
                // Validate license plate format for vehicles
                if (!string.IsNullOrEmpty(request.LicensePlate) && 
                    (request.ProductType?.ToLower().Contains("vehicle") == true || 
                     request.ProductType?.ToLower().Contains("xe") == true))
                {
                    if (!IsValidLicensePlate(request.LicensePlate))
                    {
                        return BadRequest("Invalid license plate format. Please use Vietnamese license plate format (e.g., 30A-12345, 51G-12345)");
                    }
                }

                var existingProduct = _productRepo.GetProductById(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                // Verify ownership
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (existingProduct.SellerId != userId)
                {
                    return Forbid();
                }

                existingProduct.Title = request.Title;
                existingProduct.Description = request.Description;
                existingProduct.Price = request.Price;
                existingProduct.Brand = request.Brand;
                existingProduct.Model = request.Model;
                existingProduct.Condition = request.Condition;
                existingProduct.VehicleType = request.VehicleType;
                existingProduct.ManufactureYear = request.ManufactureYear;
                existingProduct.Mileage = request.Mileage;
                existingProduct.BatteryHealth = request.BatteryHealth;
                existingProduct.BatteryType = request.BatteryType;
                existingProduct.Capacity = request.Capacity;
                existingProduct.Voltage = request.Voltage;
                existingProduct.CycleCount = request.CycleCount;
                existingProduct.LicensePlate = request.LicensePlate;

                var updatedProduct = _productRepo.UpdateProduct(existingProduct);

                var response = new
                {
                    updatedProduct.Title,
                    updatedProduct.Description,
                    updatedProduct.Price,
                    updatedProduct.Status
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpDelete("{id}")]
        //[Authorize(Policy = "MemberOnly")]
        public ActionResult DeleteProduct(int id)
        {
            try
            {
                var product = _productRepo.GetProductById(id);
                if (product == null)
                {
                    return NotFound();
                }

                // Verify ownership
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (product.SellerId != userId)
                {
                    return Forbid();
                }

                var result = _productRepo.DeleteProduct(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("seller/{sellerId}")]
        [Authorize(Policy = "MemberOnly")]
        public ActionResult GetProductsBySeller(int sellerId)
        {
            try
            {
                var products = _productRepo.GetProductsBySellerId(sellerId);

                var response = products.Select(p => new
                {
                    p.ProductId,
                    p.Title,
                    p.Price,
                    p.Status,
                    p.CreatedDate,
                    SellerName = p.Seller?.FullName,
                    ImageUrls = p.ProductImages?.Select(img => img.ImageData).ToList() ?? new List<string>()
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("drafts")]
        [Authorize(Policy = "AdminOnly")] // ✅ Có thể tùy bạn, hoặc để mở nếu cần
        public ActionResult GetDraftProducts()
        {
            try
            {
                var drafts = _productRepo.GetDraftProducts();

                var response = drafts.Select(p => new
                {
                    p.ProductId,
                    p.Title,
                    p.Price,
                    p.Status,
                    p.CreatedDate,
                    SellerName = p.Seller?.FullName,
                    ImageUrls = p.ProductImages?.Select(img => img.ImageData).ToList() ?? new List<string>()
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("approve/{id}")]
        //[Authorize(Roles = "Admin")]
        public ActionResult ApproveProduct(int id)
        {
            try
            {
                var product = _productRepo.GetProductById(id);
                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                var approved = _productRepo.ApproveProduct(id);
                if (approved == null)
                {
                    return StatusCode(500, "Failed to approve product.");
                }

                return Ok(new
                {
                    approved.ProductId,
                    approved.Title,
                    approved.Status,
                    Message = "Product approved successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("search/license-plate/{licensePlate}")]
        [AllowAnonymous]
        public ActionResult GetProductsByLicensePlate(string licensePlate)
        {
            try
            {
                var products = _productRepo.GetProductsByLicensePlate(licensePlate);

                var response = products.Select(p => new ProductResponse
                {
                    ProductId = p.ProductId,
                    SellerId = p.SellerId,
                    ProductType = p.ProductType,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    Brand = p.Brand,
                    Model = p.Model,
                    Condition = p.Condition,
                    VehicleType = p.VehicleType,
                    ManufactureYear = p.ManufactureYear,
                    Mileage = p.Mileage,
                    BatteryHealth = p.BatteryHealth,
                    BatteryType = p.BatteryType,
                    Capacity = p.Capacity,
                    Voltage = p.Voltage,
                    CycleCount = p.CycleCount,
                    LicensePlate = p.LicensePlate,
                    Status = p.Status,
                    VerificationStatus = p.VerificationStatus,
                    CreatedDate = p.CreatedDate,
                    ImageUrls = p.ProductImages?.Select(img => img.ImageData).ToList() ?? new List<string>()
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("license-plate/{licensePlate}")]
        [AllowAnonymous]
        public ActionResult GetProductByExactLicensePlate(string licensePlate)
        {
            try
            {
                var product = _productRepo.GetProductByExactLicensePlate(licensePlate);
                if (product == null)
                {
                    return NotFound("Product with this license plate not found");
                }

                var response = new ProductResponse
                {
                    ProductId = product.ProductId,
                    SellerId = product.SellerId,
                    ProductType = product.ProductType,
                    Title = product.Title,
                    Description = product.Description,
                    Price = product.Price,
                    Brand = product.Brand,
                    Model = product.Model,
                    Condition = product.Condition,
                    VehicleType = product.VehicleType,
                    ManufactureYear = product.ManufactureYear,
                    Mileage = product.Mileage,
                    BatteryHealth = product.BatteryHealth,
                    BatteryType = product.BatteryType,
                    Capacity = product.Capacity,
                    Voltage = product.Voltage,
                    CycleCount = product.CycleCount,
                    LicensePlate = product.LicensePlate,
                    Status = product.Status,
                    VerificationStatus = product.VerificationStatus,
                    CreatedDate = product.CreatedDate,
                    ImageUrls = product.ProductImages?.Select(img => img.ImageData).ToList() ?? new List<string>()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("active")]
        [AllowAnonymous] // Khách hàng có thể xem không cần login
        public ActionResult GetActiveProducts()
        {
            try
            {
                var products = _productRepo.GetActiveProducts();

                var response = products.Select(p => new ProductResponse
                {
                    ProductId = p.ProductId,
                    SellerId = p.SellerId,
                    ProductType = p.ProductType,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    Brand = p.Brand,
                    Model = p.Model,
                    Condition = p.Condition,
                    VehicleType = p.VehicleType,
                    ManufactureYear = p.ManufactureYear,
                    Mileage = p.Mileage,
                    BatteryHealth = p.BatteryHealth,
                    BatteryType = p.BatteryType,
                    Capacity = p.Capacity,
                    Voltage = p.Voltage,
                    CycleCount = p.CycleCount,
                    LicensePlate = p.LicensePlate,
                    Status = p.Status,
                    VerificationStatus = p.VerificationStatus,
                    CreatedDate = p.CreatedDate,
                    ImageUrls = p.ProductImages?.Select(img => img.ImageData).ToList() ?? new List<string>()
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        private bool IsValidLicensePlate(string licensePlate)
        {
            if (string.IsNullOrEmpty(licensePlate))
                return false;

            // Vietnamese license plate format: XX-Y.ZZZZ or XXY-ZZZZ
            // Examples: 30A-12345, 51G-12345, 29B-1234, 43C-12345
            var pattern = @"^[0-9]{2}[A-Z]{1,2}-[0-9]{4,5}$";
            return System.Text.RegularExpressions.Regex.IsMatch(licensePlate.ToUpper(), pattern);
        }
    }
}

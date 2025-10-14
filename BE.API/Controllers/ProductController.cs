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

                // ✅ Map toàn bộ thông tin cần thiết sang ProductResponse
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
                    Transmission = p.Transmission,
                    SeatCount = p.SeatCount,
                    BatteryHealth = p.BatteryHealth,
                    BatteryType = p.BatteryType,
                    Capacity = p.Capacity,
                    Voltage = p.Voltage,
                    BMS = p.BMS,
                    CellType = p.CellType,
                    CycleCount = p.CycleCount,
                    LicensePlate = p.LicensePlate,
                    Status = p.Status,
                    VerificationStatus = p.VerificationStatus,
                    RejectionReason = p.RejectionReason,
                    CreatedDate = p.CreatedDate,
                    ImageUrls = p.ProductImages.Select(img => img.ImageData).ToList()
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
                    Transmission = product.Transmission,
                    SeatCount = product.SeatCount,
                    BatteryHealth = product.BatteryHealth,
                    BatteryType = product.BatteryType,
                    Capacity = product.Capacity,
                    Voltage = product.Voltage,
                    BMS = product.BMS,
                    CellType = product.CellType,
                    CycleCount = product.CycleCount,
                    LicensePlate = product.LicensePlate,
                    Status = product.Status,
                    VerificationStatus = product.VerificationStatus,
                    RejectionReason = product.RejectionReason,
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
        [Authorize(Policy = "MemberOnly")]
        public ActionResult CreateProduct([FromBody] ProductRequest request)
        {
            try
            {
                // ✅ Validate license plate format for vehicles
                if (!string.IsNullOrEmpty(request.LicensePlate) &&
                    (request.ProductType?.ToLower().Contains("vehicle") == true ||
                     request.ProductType?.ToLower().Contains("xe") == true))
                {
                    if (!IsValidLicensePlate(request.LicensePlate))
                    {
                        return BadRequest(
                            "Invalid license plate format. Please use Vietnamese license plate format (e.g., 30A-12345, 51G-12345)");
                    }
                }

                // ✅ Tạo mới product và gán đầy đủ field
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
                    Transmission = request.Transmission,
                    SeatCount = request.SeatCount,
                    BatteryHealth = request.BatteryHealth,
                    BatteryType = request.BatteryType,
                    Capacity = request.Capacity,
                    Voltage = request.Voltage,
                    BMS = request.BMS,
                    CellType = request.CellType,
                    CycleCount = request.CycleCount,
                    LicensePlate = request.LicensePlate,

                    // ✅ Thêm các trạng thái mặc định
                    Status = "Draft",
                    VerificationStatus = "NotRequested",
                    RejectionReason = null,
                    CreatedDate = DateTime.UtcNow
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
        [Authorize(Policy = "MemberOnly")]
        public ActionResult UpdateProduct(int id, [FromBody] ProductRequest request)
        {
            try
            {
                // ✅ Validate license plate format for vehicles
                if (!string.IsNullOrEmpty(request.LicensePlate) &&
                    (request.ProductType?.ToLower().Contains("vehicle") == true ||
                     request.ProductType?.ToLower().Contains("xe") == true))
                {
                    if (!IsValidLicensePlate(request.LicensePlate))
                    {
                        return BadRequest(
                            "Invalid license plate format. Please use Vietnamese license plate format (e.g., 30A-12345, 51G-12345)");
                    }
                }

                var existingProduct = _productRepo.GetProductById(id);
                if (existingProduct == null)
                {
                    return NotFound("Product not found");
                }

                // ✅ Verify ownership
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (existingProduct.SellerId != userId)
                {
                    return Forbid();
                }

                // ✅ Cập nhật toàn bộ các trường tương tự CreateProduct
                existingProduct.ProductType = request.ProductType;
                existingProduct.Title = request.Title;
                existingProduct.Description = request.Description;
                existingProduct.Price = request.Price;
                existingProduct.Brand = request.Brand;
                existingProduct.Model = request.Model;
                existingProduct.Condition = request.Condition;
                existingProduct.VehicleType = request.VehicleType;
                existingProduct.ManufactureYear = request.ManufactureYear;
                existingProduct.Mileage = request.Mileage;
                existingProduct.Transmission = request.Transmission;
                existingProduct.SeatCount = request.SeatCount;
                existingProduct.BatteryHealth = request.BatteryHealth;
                existingProduct.BatteryType = request.BatteryType;
                existingProduct.Capacity = request.Capacity;
                existingProduct.Voltage = request.Voltage;
                existingProduct.BMS = request.BMS;
                existingProduct.CellType = request.CellType;
                existingProduct.CycleCount = request.CycleCount;
                existingProduct.LicensePlate = request.LicensePlate;

                // ✅ Reset trạng thái về "Draft" để admin duyệt lại
                existingProduct.Status = "Re-submit";
                existingProduct.VerificationStatus = "NotRequested";
                existingProduct.RejectionReason = null;

                var updatedProduct = _productRepo.UpdateProduct(existingProduct);

                var response = new
                {
                    updatedProduct.ProductId,
                    updatedProduct.Title,
                    updatedProduct.Price,
                    updatedProduct.Status,
                    updatedProduct.VerificationStatus,
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
                    p.ProductType,
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
        [Authorize(Policy = "AdminOnly")]
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
                    approved.VerificationStatus,
                    Message = "Product approved successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("reject/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult RejectProduct(int id, [FromBody] RejectProductRequest? request = null)
        {
            try
            {
                var product = _productRepo.GetProductById(id);
                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                var rejected = _productRepo.RejectProduct(id, request?.RejectionReason);
                if (rejected == null)
                {
                    return StatusCode(500, "Failed to reject product.");
                }

                return Ok(new
                {
                    rejected.ProductId,
                    rejected.Title,
                    rejected.Status,
                    rejected.VerificationStatus,
                    rejected.RejectionReason,
                    Message = "Product rejected successfully."
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

        [HttpGet("vehicles")]
        [AllowAnonymous]
        public ActionResult GetVehicles()
        {
            try
            {
                var vehicles = _productRepo.GetProductsByType("vehicle");

                var response = vehicles.Select(p => new VehicleResponse
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
                    Transmission = p.Transmission,
                    SeatCount = p.SeatCount,
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

        [HttpGet("batteries")]
        [AllowAnonymous]
        public ActionResult GetBatteries()
        {
            try
            {
                var batteries = _productRepo.GetProductsByType("battery");

                var response = batteries.Select(p => new BatteryResponse
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
                    BatteryType = p.BatteryType,
                    BatteryHealth = p.BatteryHealth,
                    Capacity = p.Capacity,
                    Voltage = p.Voltage,
                    BMS = p.BMS,
                    CellType = p.CellType,
                    CycleCount = p.CycleCount,
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

        [HttpPost("vehicles")]
        [Authorize(Policy = "MemberOnly")]
        public ActionResult CreateVehicle([FromBody] VehicleRequest request)
        {
            try
            {
                // Validate license plate format for vehicles
                if (!string.IsNullOrEmpty(request.LicensePlate))
                {
                    if (!IsValidLicensePlate(request.LicensePlate))
                    {
                        return BadRequest(
                            "Invalid license plate format. Please use Vietnamese license plate format (e.g., 30A-12345, 51G-12345)");
                    }
                }

                var product = new Product
                {
                    SellerId = int.TryParse(User.FindFirst("UserId")?.Value, out var userId) ? userId : 0,
                    ProductType = "Vehicle",
                    Title = request.Title,
                    Description = request.Description,
                    Price = request.Price,
                    Brand = request.Brand,
                    Model = request.Model,
                    Condition = request.Condition,
                    VehicleType = request.VehicleType,
                    ManufactureYear = request.ManufactureYear,
                    Mileage = request.Mileage,
                    Transmission = request.Transmission,
                    SeatCount = request.SeatCount,
                    LicensePlate = request.LicensePlate
                };

                var createdVehicle = _productRepo.CreateProduct(product);

                var response = new VehicleResponse
                {
                    ProductId = createdVehicle.ProductId,
                    SellerId = createdVehicle.SellerId,
                    ProductType = createdVehicle.ProductType,
                    Title = createdVehicle.Title,
                    Description = createdVehicle.Description,
                    Price = createdVehicle.Price,
                    Brand = createdVehicle.Brand,
                    Model = createdVehicle.Model,
                    Condition = createdVehicle.Condition,
                    VehicleType = createdVehicle.VehicleType,
                    ManufactureYear = createdVehicle.ManufactureYear,
                    Mileage = createdVehicle.Mileage,
                    Transmission = createdVehicle.Transmission,
                    SeatCount = createdVehicle.SeatCount,
                    LicensePlate = createdVehicle.LicensePlate,
                    Status = createdVehicle.Status,
                    VerificationStatus = createdVehicle.VerificationStatus,
                    CreatedDate = createdVehicle.CreatedDate,
                    ImageUrls = new List<string>()
                };

                return CreatedAtAction(nameof(GetProductById), new { id = createdVehicle.ProductId }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost("batteries")]
        [Authorize(Policy = "MemberOnly")]
        public ActionResult CreateBattery([FromBody] BatteryRequest request)
        {
            try
            {
                var product = new Product
                {
                    SellerId = int.TryParse(User.FindFirst("UserId")?.Value, out var userId) ? userId : 0,
                    ProductType = "Battery",
                    Title = request.Title,
                    Description = request.Description,
                    Price = request.Price,
                    Brand = request.Brand,
                    Model = request.Model,
                    Condition = request.Condition,
                    BatteryType = request.BatteryType,
                    BatteryHealth = request.BatteryHealth,
                    Capacity = request.Capacity,
                    Voltage = request.Voltage,
                    BMS = request.BMS,
                    CellType = request.CellType,
                    CycleCount = request.CycleCount
                };

                var createdBattery = _productRepo.CreateProduct(product);

                var response = new BatteryResponse
                {
                    ProductId = createdBattery.ProductId,
                    SellerId = createdBattery.SellerId,
                    ProductType = createdBattery.ProductType,
                    Title = createdBattery.Title,
                    Description = createdBattery.Description,
                    Price = createdBattery.Price,
                    Brand = createdBattery.Brand,
                    Model = createdBattery.Model,
                    Condition = createdBattery.Condition,
                    BatteryType = createdBattery.BatteryType,
                    BatteryHealth = createdBattery.BatteryHealth,
                    Capacity = createdBattery.Capacity,
                    Voltage = createdBattery.Voltage,
                    BMS = createdBattery.BMS,
                    CellType = createdBattery.CellType,
                    CycleCount = createdBattery.CycleCount,
                    Status = createdBattery.Status,
                    VerificationStatus = createdBattery.VerificationStatus,
                    CreatedDate = createdBattery.CreatedDate,
                    ImageUrls = new List<string>()
                };

                return CreatedAtAction(nameof(GetProductById), new { id = createdBattery.ProductId }, response);
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


        [HttpPut("vehicles/{id}")]
        [Authorize(Policy = "MemberOnly")]
        public ActionResult UpdateVehicle(int id, [FromBody] VehicleRequest request)
        {
            try
            {
                var existingProduct = _productRepo.GetProductById(id);
                if (existingProduct == null)
                    return NotFound("Vehicle not found.");

                // Xác nhận đây là sản phẩm loại "Vehicle"
                if (!string.Equals(existingProduct.ProductType, "Vehicle", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Product is not a vehicle.");

                // Kiểm tra quyền sở hữu (chủ sản phẩm)
                var userId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : 0;
                if (existingProduct.SellerId != userId && userId != 0)
                    return Forbid();

                // Kiểm tra định dạng biển số xe
                if (!string.IsNullOrEmpty(request.LicensePlate))
                {
                    if (!IsValidLicensePlate(request.LicensePlate))
                        return BadRequest("Invalid license plate format (e.g., 30A-12345, 51G-12345)");
                }

                // Cập nhật dữ liệu
                existingProduct.Title = request.Title;
                existingProduct.Description = request.Description;
                existingProduct.Price = request.Price;
                existingProduct.Brand = request.Brand;
                existingProduct.Model = request.Model;
                existingProduct.Condition = request.Condition;
                existingProduct.VehicleType = request.VehicleType;
                existingProduct.ManufactureYear = request.ManufactureYear;
                existingProduct.Mileage = request.Mileage;
                existingProduct.Transmission = request.Transmission;
                existingProduct.SeatCount = request.SeatCount;
                existingProduct.LicensePlate = request.LicensePlate;

                existingProduct.Status = "Draft";
                existingProduct.VerificationStatus = "NotRequested";

                var updatedVehicle = _productRepo.UpdateProduct(existingProduct);

                var response = new
                {
                    updatedVehicle.ProductId,
                    updatedVehicle.Title,
                    updatedVehicle.Price,
                    updatedVehicle.Status,
                    updatedVehicle.VerificationStatus,
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("batteries/{id}")]
        [Authorize(Policy = "MemberOnly")]
        public ActionResult UpdateBattery(int id, [FromBody] BatteryRequest request)
        {
            try
            {
                var existingProduct = _productRepo.GetProductById(id);
                if (existingProduct == null)
                    return NotFound("Battery not found.");

                // Xác nhận đây là sản phẩm loại "Battery"
                if (!string.Equals(existingProduct.ProductType, "Battery", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Product is not a battery.");

                // Kiểm tra quyền sở hữu
                var userId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : 0;
                if (existingProduct.SellerId != userId && userId != 0)
                    return Forbid();

                // Cập nhật dữ liệu
                existingProduct.Title = request.Title;
                existingProduct.Description = request.Description;
                existingProduct.Price = request.Price;
                existingProduct.Brand = request.Brand;
                existingProduct.Model = request.Model;
                existingProduct.Condition = request.Condition;
                existingProduct.BatteryType = request.BatteryType;
                existingProduct.BatteryHealth = request.BatteryHealth;
                existingProduct.Capacity = request.Capacity;
                existingProduct.Voltage = request.Voltage;
                existingProduct.BMS = request.BMS;
                existingProduct.CellType = request.CellType;
                existingProduct.CycleCount = request.CycleCount;

                existingProduct.Status = "Draft";
                existingProduct.VerificationStatus = "NotRequested";

                var updatedBattery = _productRepo.UpdateProduct(existingProduct);

                var response = new
                {
                    updatedBattery.ProductId,
                    updatedBattery.Title,
                    updatedBattery.Price,
                    updatedBattery.Status,
                    updatedBattery.VerificationStatus,
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("batteries/{id}")]
        [AllowAnonymous]
        public ActionResult GetBatteryById(int id)
        {
            try
            {
                var product = _productRepo.GetProductById(id);
                if (product == null ||
                    !string.Equals(product.ProductType, "Battery", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound("Battery not found.");
                }

                var response = new BatteryResponse
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
                    BatteryType = product.BatteryType,
                    BatteryHealth = product.BatteryHealth,
                    Capacity = product.Capacity,
                    Voltage = product.Voltage,
                    BMS = product.BMS,
                    CellType = product.CellType,
                    CycleCount = product.CycleCount,
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

        [HttpGet("vehicles/{id}")]
        [AllowAnonymous]
        public ActionResult GetVehicleById(int id)
        {
            try
            {
                var product = _productRepo.GetProductById(id);
                if (product == null ||
                    !string.Equals(product.ProductType, "Vehicle", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound("Vehicle not found.");
                }

                var response = new VehicleResponse
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
                    Transmission = product.Transmission,
                    SeatCount = product.SeatCount,
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
        
        [HttpGet("resubmit")]
        [Authorize(Policy = "AdminOnly")] // hoặc bỏ nếu bạn chưa có phân quyền
        public ActionResult GetReSubmittedProducts()
        {
            try
            {
                var products = _productRepo.GetReSubmittedProducts();

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
                    Transmission = p.Transmission,
                    SeatCount = p.SeatCount,
                    BatteryHealth = p.BatteryHealth,
                    BatteryType = p.BatteryType,
                    Capacity = p.Capacity,
                    Voltage = p.Voltage,
                    BMS = p.BMS,
                    CellType = p.CellType,
                    CycleCount = p.CycleCount,
                    LicensePlate = p.LicensePlate,
                    Status = p.Status,
                    VerificationStatus = p.VerificationStatus,
                    RejectionReason = p.RejectionReason,
                    CreatedDate = p.CreatedDate,
                    ImageUrls = p.ProductImages.Select(img => img.ImageData).ToList()
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
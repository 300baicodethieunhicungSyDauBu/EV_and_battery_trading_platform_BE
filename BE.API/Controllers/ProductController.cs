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
        private readonly IOrderRepo _orderRepo;

        public ProductController(IProductRepo productRepo, IOrderRepo orderRepo)
        {
            _productRepo = productRepo;
            _orderRepo = orderRepo;
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
        public ActionResult DeleteProduct(int id)
        {
            try
            {
                var product = _productRepo.GetProductById(id);
                if (product == null)
                {
                    return NotFound();
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

                var response = products
                    .OrderByDescending(p => p.CreatedDate) // ✅ Sort by CreatedDate descending (newest first)
                    .Select(p => new
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

        [HttpGet("admin/all-statuses")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult GetAllProductStatuses()
        {
            try
            {
                var products = _productRepo.GetAllProducts();

                var statusGroups = products.GroupBy(p => p.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Count = g.Count(),
                        Description = GetStatusDescription(g.Key, null),
                        Products = g.Select(p => new
                        {
                            p.ProductId,
                            p.Title,
                            p.VerificationStatus,
                            p.SellerId,
                            p.CreatedDate
                        }).ToList()
                    })
                    .OrderBy(g => g.Status)
                    .ToList();

                var response = new
                {
                    TotalProducts = products.Count,
                    StatusGroups = statusGroups,
                    StatusMapping = new
                    {
                        Draft = "Chờ duyệt - Admin chưa duyệt",
                        Active = "Đang bán - Có thể mua",
                        Reserved = "Đã có đơn hàng - Chờ thanh toán deposit",
                        Sold = "Đã bán - Không thể mua nữa",
                        Rejected = "Đã từ chối - Cần seller chỉnh sửa",
                        Deleted = "Đã xóa"
                    },
                    Message = "Admin dashboard nên hiển thị: Reserved = 'Đã có đơn hàng', Draft = 'Chờ duyệt'"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Get all statuses error: " + ex.Message);
            }
        }

        [HttpPost("test-update-status/{productId}")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult TestUpdateProductStatus(int productId, [FromBody] TestUpdateStatusRequest request)
        {
            try
            {
                var product = _productRepo.GetProductById(productId);
                if (product == null)
                {
                    return NotFound($"Product with ID {productId} not found");
                }

                var oldStatus = product.Status;
                product.Status = request.NewStatus;
                var updatedProduct = _productRepo.UpdateProduct(product);

                var response = new
                {
                    ProductId = updatedProduct.ProductId,
                    Title = updatedProduct.Title,
                    OldStatus = oldStatus,
                    NewStatus = updatedProduct.Status,
                    StatusDescription = GetStatusDescription(updatedProduct.Status, updatedProduct.VerificationStatus),
                    UpdatedDate = DateTime.Now,
                    Message = $"Product status updated from '{oldStatus}' to '{request.NewStatus}'"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Test update error: " + ex.Message);
            }
        }

        [HttpGet("debug/status/{productId}")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult DebugProductStatus(int productId)
        {
            try
            {
                var product = _productRepo.GetProductById(productId);
                if (product == null)
                {
                    return NotFound($"Product with ID {productId} not found");
                }

                // Lấy thông tin Order liên quan
                var orders = _orderRepo.GetAllOrders()
                    .Where(o => o.ProductId == productId)
                    .ToList();

                var response = new
                {
                    ProductId = product.ProductId,
                    Title = product.Title,
                    Status = product.Status,
                    VerificationStatus = product.VerificationStatus,
                    StatusDescription = GetStatusDescription(product.Status, product.VerificationStatus),
                    CreatedDate = product.CreatedDate,
                    SellerId = product.SellerId,
                    SellerName = product.Seller?.FullName,

                    // Thông tin Order liên quan
                    RelatedOrders = orders.Select(o => new
                    {
                        o.OrderId,
                        o.Status,
                        o.DepositStatus,
                        o.FinalPaymentStatus,
                        o.BuyerId,
                        o.SellerId,
                        o.CreatedDate
                    }).ToList(),

                    // Thông tin Payment liên quan
                    RelatedPayments = orders.SelectMany(o => o.Payments ?? new List<Payment>()).Select(p => new
                    {
                        p.PaymentId,
                        p.PaymentType,
                        p.Status,
                        p.Amount,
                        p.CreatedDate,
                        p.PayDate
                    }).ToList(),

                    DebugInfo = new
                    {
                        ExpectedStatus = "Reserved (có đơn hàng, chờ thanh toán)",
                        ActualStatus = product.Status,
                        IsCorrect = product.Status == "Reserved",
                        ShouldShowAs = product.Status == "Reserved" ? "Đã có đơn hàng" : "Chờ duyệt"
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Debug error: " + ex.Message);
            }
        }

        [HttpGet("admin/status/{status}")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult GetProductsByStatus(string status)
        {
            try
            {
                var products = _productRepo.GetAllProducts()
                    .Where(p => p.Status != null && p.Status.ToLower() == status.ToLower())
                    .ToList();

                var response = products.Select(p => new
                {
                    p.ProductId,
                    p.Title,
                    p.Price,
                    p.Status,
                    p.VerificationStatus,
                    p.CreatedDate,
                    SellerName = p.Seller?.FullName,
                    SellerId = p.SellerId,
                    ImageUrls = p.ProductImages?.Select(img => img.ImageData).ToList() ?? new List<string>(),
                    // Thêm thông tin để phân biệt trạng thái
                    StatusDescription = GetStatusDescription(p.Status, p.VerificationStatus)
                }).ToList();

                return Ok(new
                {
                    Status = status,
                    Count = products.Count,
                    Products = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        private string GetStatusDescription(string? status, string? verificationStatus)
        {
            return status?.ToLower() switch
            {
                "draft" => "Bản nháp - Chờ seller hoàn thiện",
                "re-submit" => "Chờ duyệt lại - Seller đã chỉnh sửa",
                "rejected" => "Đã từ chối - Cần seller chỉnh sửa",
                "active" => "Đang bán - Có thể mua",
                "reserved" => "Đã có đơn hàng - Chờ thanh toán deposit",
                "sold" => "Đã bán - Không thể mua nữa",
                "deleted" => "Đã xóa",
                _ => $"Trạng thái không xác định: {status}"
            };
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

        [HttpPut("resubmit/{id}")]
        [Authorize(Policy = "MemberOnly")]
        public ActionResult ResubmitProduct(int id)
        {
            try
            {
                var product = _productRepo.GetProductById(id);
                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                // Verify ownership
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (product.SellerId != userId)
                {
                    return Forbid("You can only resubmit your own products.");
                }

                // Check if product is in rejected status
                if (product.Status != "Rejected")
                {
                    return BadRequest("Only rejected products can be resubmitted.");
                }

                var resubmittedProduct = _productRepo.ResubmitProduct(id);
                if (resubmittedProduct == null)
                {
                    return StatusCode(500, "Failed to resubmit product.");
                }

                return Ok(new
                {
                    resubmittedProduct.ProductId,
                    resubmittedProduct.Title,
                    resubmittedProduct.Status,
                    resubmittedProduct.VerificationStatus,
                    Message = "Product resubmitted successfully. Waiting for admin review."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("seller/{sellerId}/rejected")]
        [Authorize(Policy = "MemberOnly")]
        public ActionResult GetRejectedProductsBySeller(int sellerId)
        {
            try
            {
                // Verify that user can only access their own rejected products
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("Invalid user token.");
                }

                if (userId != sellerId)
                {
                    return Forbid($"Access denied. UserId from token: {userId}, SellerId from URL: {sellerId}");
                }

                // Debug: Get all products by seller first to check
                var allProducts = _productRepo.GetProductsBySellerId(sellerId);

                var products = _productRepo.GetRejectedProductsBySellerId(sellerId);

                // Debug response with more information
                var debugResponse = new
                {
                    DebugInfo = new
                    {
                        RequestedSellerId = sellerId,
                        UserIdFromToken = userId,
                        AllProductsCount = allProducts.Count,
                        RejectedProductsCount = products.Count,
                        AllProductsStatuses = allProducts.Select(p => new
                            { p.ProductId, p.Status, p.VerificationStatus, p.RejectionReason }).ToList()
                    },
                    RejectedProducts = products.Select(p => new ProductResponse
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
                        ImageUrls = p.ProductImages?.Select(img => img.ImageData).ToList() ?? new List<string>()
                    }).ToList()
                };

                return Ok(debugResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("verification/requested")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult GetRequestedVerificationProducts()
        {
            try
            {
                var products = _productRepo.GetAllProducts()
                    .Where(p => p.VerificationStatus == "Requested")
                    .ToList();

                if (!products.Any())
                    return NotFound("No products with VerificationStatus = 'Requested'.");

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


        [HttpPut("verify/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult VerifyProduct(int id)
        {
            try
            {
                var product = _productRepo.GetProductById(id);
                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                if (product.VerificationStatus != "Requested")
                {
                    return BadRequest("Only products with VerificationStatus = 'Requested' can be verified.");
                }

                product.VerificationStatus = "Verified";
                product.Status = "Active"; // optional: tự động kích hoạt
                _productRepo.UpdateProduct(product);

                return Ok(new
                {
                    product.ProductId,
                    product.Title,
                    product.Status,
                    product.VerificationStatus,
                    Message = "Product verified successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost("search")]
        [AllowAnonymous]
        public ActionResult SearchProducts([FromBody] ProductSearchRequest request)
        {
            try
            {
                var products = _productRepo.GetAllProducts().AsQueryable();

                // 🔍 1️⃣ Keyword search (case-insensitive)
                if (!string.IsNullOrWhiteSpace(request.Keyword))
                {
                    var keyword = request.Keyword.ToLower();
                    products = products.Where(p =>
                        (p.Title != null && p.Title.ToLower().Contains(keyword)) ||
                        (p.Brand != null && p.Brand.ToLower().Contains(keyword)) ||
                        (p.Model != null && p.Model.ToLower().Contains(keyword))
                    );
                }

                // 🔍 2️⃣ ProductType (Vehicle / Battery)
                if (!string.IsNullOrEmpty(request.ProductType))
                {
                    var type = request.ProductType.ToLower();
                    products = products.Where(p => p.ProductType.ToLower() == type);
                }

                // 🔍 3️⃣ Brand, Model, Condition (case-insensitive)
                if (!string.IsNullOrEmpty(request.Brand))
                {
                    var brand = request.Brand.ToLower();
                    products = products.Where(p => p.Brand != null && p.Brand.ToLower().Contains(brand));
                }

                if (!string.IsNullOrEmpty(request.Model))
                {
                    var model = request.Model.ToLower();
                    products = products.Where(p => p.Model != null && p.Model.ToLower().Contains(model));
                }

                if (!string.IsNullOrEmpty(request.Condition))
                {
                    var condition = request.Condition.ToLower();
                    products = products.Where(p => p.Condition != null && p.Condition.ToLower().Contains(condition));
                }

                // 💰 4️⃣ Price Range
                if (request.MinPrice.HasValue)
                    products = products.Where(p => p.Price >= request.MinPrice.Value);
                if (request.MaxPrice.HasValue)
                    products = products.Where(p => p.Price <= request.MaxPrice.Value);

                // 🚗 5️⃣ Vehicle-specific filters
                if (!string.IsNullOrEmpty(request.VehicleType))
                {
                    var vtype = request.VehicleType.ToLower();
                    products = products.Where(p => p.VehicleType != null && p.VehicleType.ToLower().Contains(vtype));
                }

                if (!string.IsNullOrEmpty(request.Transmission))
                {
                    var transmission = request.Transmission.ToLower();
                    products = products.Where(p =>
                        p.Transmission != null && p.Transmission.ToLower().Contains(transmission));
                }

                if (request.MinManufactureYear.HasValue)
                    products = products.Where(p => p.ManufactureYear >= request.MinManufactureYear.Value);
                if (request.MaxManufactureYear.HasValue)
                    products = products.Where(p => p.ManufactureYear <= request.MaxManufactureYear.Value);

                if (request.MaxMileage.HasValue)
                    products = products.Where(p => p.Mileage <= request.MaxMileage.Value);

                if (request.SeatCount.HasValue)
                    products = products.Where(p => p.SeatCount == request.SeatCount.Value);

                // 🔋 6️⃣ Battery-specific filters
                if (!string.IsNullOrEmpty(request.BatteryType))
                {
                    var btype = request.BatteryType.ToLower();
                    products = products.Where(p => p.BatteryType != null && p.BatteryType.ToLower().Contains(btype));
                }

                if (request.MinBatteryHealth.HasValue)
                    products = products.Where(p => p.BatteryHealth >= request.MinBatteryHealth.Value);
                if (request.MaxBatteryHealth.HasValue)
                    products = products.Where(p => p.BatteryHealth <= request.MaxBatteryHealth.Value);

                if (request.MinCapacity.HasValue)
                    products = products.Where(p => p.Capacity >= request.MinCapacity.Value);
                if (request.MaxCapacity.HasValue)
                    products = products.Where(p => p.Capacity <= request.MaxCapacity.Value);

                if (!string.IsNullOrEmpty(request.CellType))
                {
                    var cell = request.CellType.ToLower();
                    products = products.Where(p => p.CellType != null && p.CellType.ToLower().Contains(cell));
                }

                if (!string.IsNullOrEmpty(request.BMS))
                {
                    var bms = request.BMS.ToLower();
                    products = products.Where(p => p.BMS != null && p.BMS.ToLower().Contains(bms));
                }

                // ⚙️ 7️⃣ Status filters
                if (!string.IsNullOrEmpty(request.Status))
                {
                    var status = request.Status.ToLower();
                    products = products.Where(p => p.Status != null && p.Status.ToLower() == status);
                }

                if (!string.IsNullOrEmpty(request.VerificationStatus))
                {
                    var verStatus = request.VerificationStatus.ToLower();
                    products = products.Where(p =>
                        p.VerificationStatus != null && p.VerificationStatus.ToLower() == verStatus);
                }

                // ✅ 8️⃣ Map sang Response
                var result = products.Select(p => new ProductResponse
                {
                    ProductId = p.ProductId,
                    SellerId = p.SellerId,
                    ProductType = p.ProductType,
                    Title = p.Title,
                    Brand = p.Brand,
                    Model = p.Model,
                    Condition = p.Condition,
                    Price = p.Price,
                    VehicleType = p.VehicleType,
                    ManufactureYear = p.ManufactureYear,
                    Mileage = p.Mileage,
                    Transmission = p.Transmission,
                    SeatCount = p.SeatCount,
                    BatteryType = p.BatteryType,
                    BatteryHealth = p.BatteryHealth,
                    Capacity = p.Capacity,
                    Voltage = p.Voltage,
                    CellType = p.CellType,
                    BMS = p.BMS,
                    Status = p.Status,
                    VerificationStatus = p.VerificationStatus,
                    ImageUrls = p.ProductImages.Select(img => img.ImageData).ToList()
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("admin/update/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult AdminUpdateProduct(int id, [FromBody] ProductRequest request)
        {
            try
            {
                var existingProduct = _productRepo.GetProductById(id);
                if (existingProduct == null)
                    return NotFound("Product not found.");

                // ✅ Lưu lại trạng thái hiện tại
                var currentStatus = existingProduct.Status;
                var currentVerification = existingProduct.VerificationStatus;

                // ✅ Cập nhật các trường được phép chỉnh sửa
                existingProduct.Title = request.Title ?? existingProduct.Title;
                existingProduct.Description = request.Description ?? existingProduct.Description;
                existingProduct.Price = request.Price != 0 ? request.Price : existingProduct.Price;
                existingProduct.Brand = request.Brand ?? existingProduct.Brand;
                existingProduct.Model = request.Model ?? existingProduct.Model;
                existingProduct.Condition = request.Condition ?? existingProduct.Condition;

                // Nếu là xe
                existingProduct.VehicleType = request.VehicleType ?? existingProduct.VehicleType;
                existingProduct.ManufactureYear = request.ManufactureYear ?? existingProduct.ManufactureYear;
                existingProduct.Mileage = request.Mileage ?? existingProduct.Mileage;
                existingProduct.Transmission = request.Transmission ?? existingProduct.Transmission;
                existingProduct.SeatCount = request.SeatCount ?? existingProduct.SeatCount;
                existingProduct.LicensePlate = request.LicensePlate ?? existingProduct.LicensePlate;

                // Nếu là pin
                existingProduct.BatteryType = request.BatteryType ?? existingProduct.BatteryType;
                existingProduct.BatteryHealth = request.BatteryHealth ?? existingProduct.BatteryHealth;
                existingProduct.Capacity = request.Capacity ?? existingProduct.Capacity;
                existingProduct.Voltage = request.Voltage ?? existingProduct.Voltage;
                existingProduct.BMS = request.BMS ?? existingProduct.BMS;
                existingProduct.CellType = request.CellType ?? existingProduct.CellType;
                existingProduct.CycleCount = request.CycleCount ?? existingProduct.CycleCount;

                // ✅ Giữ nguyên trạng thái
                existingProduct.Status = currentStatus;
                existingProduct.VerificationStatus = currentVerification;

                var updatedProduct = _productRepo.UpdateProduct(existingProduct);

                var response = new ProductResponse
                {
                    ProductId = updatedProduct.ProductId,
                    SellerId = updatedProduct.SellerId,
                    ProductType = updatedProduct.ProductType,
                    Title = updatedProduct.Title,
                    Description = updatedProduct.Description,
                    Price = updatedProduct.Price,
                    Brand = updatedProduct.Brand,
                    Model = updatedProduct.Model,
                    Condition = updatedProduct.Condition,
                    VehicleType = updatedProduct.VehicleType,
                    ManufactureYear = updatedProduct.ManufactureYear,
                    Mileage = updatedProduct.Mileage,
                    Transmission = updatedProduct.Transmission,
                    SeatCount = updatedProduct.SeatCount,
                    BatteryType = updatedProduct.BatteryType,
                    BatteryHealth = updatedProduct.BatteryHealth,
                    Capacity = updatedProduct.Capacity,
                    Voltage = updatedProduct.Voltage,
                    BMS = updatedProduct.BMS,
                    CellType = updatedProduct.CellType,
                    CycleCount = updatedProduct.CycleCount,
                    LicensePlate = updatedProduct.LicensePlate,
                    Status = updatedProduct.Status,
                    VerificationStatus = updatedProduct.VerificationStatus,
                    CreatedDate = updatedProduct.CreatedDate,
                    ImageUrls = updatedProduct.ProductImages?.Select(img => img.ImageData).ToList() ??
                                new List<string>()
                };

                return Ok(new
                {
                    Message = "Product updated successfully (admin edit, no status change).",
                    Product = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
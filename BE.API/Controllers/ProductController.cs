using BE.API.DTOs.Request;
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
                var response = products.Select(p => new
                {
                    p.ProductId,
                    p.ProductType,
                    p.Title,
                    p.Description,
                    p.Price,
                    p.Brand,
                    p.Model,
                    p.Condition,
                    p.VehicleType,
                    p.ManufactureYear,
                    p.Mileage,
                    p.BatteryHealth,
                    p.BatteryType,
                    p.Capacity,
                    p.Voltage,
                    p.CycleCount,
                    p.Status,
                    p.VerificationStatus,
                    p.CreatedDate,
                    SellerName = p.Seller?.FullName
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
                {
                    return NotFound();
                }

                var response = new
                {
                    product.ProductId,
                    product.ProductType,
                    product.Title,
                    product.Description,
                    product.Price,
                    product.Brand,
                    product.Model,
                    product.Condition,
                    product.VehicleType,
                    product.ManufactureYear,
                    product.Mileage,
                    product.BatteryHealth,
                    product.BatteryType,
                    product.Capacity,
                    product.Voltage,
                    product.CycleCount,
                    product.Status,
                    product.VerificationStatus,
                    product.CreatedDate,
                    SellerName = product.Seller?.FullName
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
                var product = new Product
                {
                    SellerId = int.Parse(User.FindFirst("UserId")?.Value ?? "0"),
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
                    CycleCount = request.CycleCount
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
                    SellerName = p.Seller?.FullName
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("drafts")]
        //[Authorize(Roles = "Admin")] // nếu bạn dùng role-based auth
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
                    SellerName = p.Seller?.FullName
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

        [HttpGet("active")]
        public ActionResult GetActiveProducts()
        {
            try
            {
                var actives = _productRepo.GetActiveProducts();
                var response = actives.Select(p => new
                {
                    p.ProductId,
                    p.Title,
                    p.Price,
                    p.Brand,
                    p.Model,
                    p.VehicleType,
                    p.BatteryType,
                    p.CreatedDate,
                    SellerName = p.Seller?.FullName
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}

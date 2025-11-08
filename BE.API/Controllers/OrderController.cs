using BE.API.DTOs.Request;
using BE.BOs.Models;
using BE.REPOs.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepo _orderRepo;
        private readonly IUserRepo _userRepo;
        private readonly IProductRepo _productRepo;

        public OrderController(IOrderRepo orderRepo, IUserRepo userRepo, IProductRepo productRepo)
        {
            _orderRepo = orderRepo;
            _userRepo = userRepo;
            _productRepo = productRepo;
        }

        [HttpGet]
        //[Authorize(Policy = "AdminOnly")]
        public ActionResult GetAllOrders()
        {
            try
            {
                var orders = _orderRepo.GetAllOrders();
                var response = orders.Select(o => new
                {
                    o.OrderId,
                    o.BuyerId,
                    o.SellerId,
                    o.ProductId,
                    o.TotalAmount,
                    o.DepositAmount,
                    o.Status,
                    o.DepositStatus,
                    o.FinalPaymentStatus,
                    o.PayoutAmount,
                    o.PayoutStatus,
                    o.CreatedDate,
                    o.CompletedDate,
                    o.CancellationReason,
                    o.CancelledDate,
                    BuyerName = o.Buyer?.FullName,
                    SellerName = o.Seller?.FullName,
                    Product = new
                    {
                        o.Product?.Title,
                        o.Product?.Price
                    },
                    PaymentsCount = o.Payments?.Count ?? 0
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("{id}")]
        public ActionResult GetOrderById(int id)
        {
            try
            {
                var order = _orderRepo.GetOrderById(id);
                if (order == null)
                {
                    return NotFound();
                }

                // Verify if user has access to this order
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (order.BuyerId != userId && order.SellerId != userId && !User.IsInRole("1"))
                {
                    return Forbid();
                }

                var response = new
                {
                    order.OrderId,
                    order.TotalAmount,
                    order.DepositAmount,
                    order.Status,
                    order.DepositStatus,
                    order.FinalPaymentStatus,
                    order.PayoutAmount,
                    order.PayoutStatus,
                    order.CreatedDate,
                    order.CompletedDate,
                    order.CancellationReason,
                    order.CancelledDate,
                    BuyerName = order.Buyer?.FullName,
                    SellerName = order.Seller?.FullName,
                    Product = new
                    {
                        order.Product?.Title,
                        order.Product?.Price
                    },
                    Payments = order.Payments?.Select(p => new
                    {
                        p.PaymentId,
                        p.Amount,
                        p.PaymentType,
                        p.Status,
                        p.CreatedDate
                    })
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
        public ActionResult CreateOrder([FromBody] OrderRequest request)
        {
            try
            {
                // Validation
                if (request.SellerId <= 0)
                    return BadRequest("Valid SellerId is required.");
                if (request.ProductId <= 0)
                    return BadRequest("Valid ProductId is required.");
                if (request.TotalAmount <= 0)
                    return BadRequest("TotalAmount must be greater than 0.");
                if (request.DepositAmount < 0)
                    return BadRequest("DepositAmount cannot be negative.");

                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (userId <= 0) return Unauthorized("Invalid user token.");

                // Validate foreign key existence
                var buyerId = request.BuyerId ?? userId;
                
                // Check if Seller exists
                var seller = _userRepo.GetUserById(request.SellerId!.Value);
                if (seller == null)
                    return BadRequest($"Seller with ID {request.SellerId} does not exist.");

                // Check if Product exists
                var product = _productRepo.GetProductById(request.ProductId!.Value);
                if (product == null)
                    return BadRequest($"Product with ID {request.ProductId} does not exist.");

                // Check if Buyer exists (if different from current user)
                if (buyerId != userId)
                {
                    var buyer = _userRepo.GetUserById(buyerId);
                    if (buyer == null)
                        return BadRequest($"Buyer with ID {buyerId} does not exist.");
                }

                var order = new Order
                {
                    BuyerId = buyerId, // Use validated buyerId
                    SellerId = request.SellerId,
                    ProductId = request.ProductId,
                    TotalAmount = request.TotalAmount,
                    DepositAmount = request.DepositAmount,
                    Status = request.Status ?? "Pending",
                    DepositStatus = request.DepositStatus ?? "Unpaid",
                    FinalPaymentStatus = request.FinalPaymentStatus ?? "Unpaid",
                    FinalPaymentDueDate = request.FinalPaymentDueDate,
                    PayoutAmount = request.PayoutAmount ?? 0,
                    PayoutStatus = request.PayoutStatus ?? "Pending",
                    CreatedDate = DateTime.Now
                };

                var createdOrder = _orderRepo.CreateOrder(order);

                var response = new
                {
                    createdOrder.OrderId,
                    createdOrder.BuyerId,
                    createdOrder.SellerId,
                    createdOrder.ProductId,
                    createdOrder.TotalAmount,
                    createdOrder.DepositAmount,
                    createdOrder.Status,
                    createdOrder.DepositStatus,
                    createdOrder.FinalPaymentStatus,
                    createdOrder.CreatedDate
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("{id}/status")]
        public ActionResult UpdateOrderStatus(int id, [FromBody] OrderRequest request)
        {
            try
            {
                var order = _orderRepo.GetOrderById(id);
                if (order == null)
                {
                    return NotFound();
                }

                // Verify if user has access to update this order
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (order.BuyerId != userId && order.SellerId != userId && !User.IsInRole("1"))
                {
                    return Forbid();
                }

                // Lưu trạng thái cũ để so sánh
                var oldStatus = order.Status;
                order.Status = request.Status;
                var updatedOrder = _orderRepo.UpdateOrder(order);

                // Logic cập nhật Product status khi seller xác nhận
                if (order.SellerId == userId && 
                    (request.Status == "Confirmed" || request.Status == "Completed") &&
                    oldStatus != request.Status &&
                    order.ProductId.HasValue)
                {
                    var product = _productRepo.GetProductById(order.ProductId.Value);
                    if (product != null && product.Status == "Reserved")
                    {
                        product.Status = "Sold";
                        _productRepo.UpdateProduct(product);
                    }
                }

                var response = new
                {
                    updatedOrder.OrderId,
                    updatedOrder.Status,
                    UpdatedDate = DateTime.Now,
                    ProductStatusUpdated = (order.SellerId == userId && 
                                          (request.Status == "Confirmed" || request.Status == "Completed") &&
                                          oldStatus != request.Status &&
                                          order.ProductId.HasValue)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost("test-seller-confirm/{orderId}")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult TestSellerConfirm(int orderId, [FromBody] TestSellerConfirmRequest request)
        {
            try
            {
                var order = _orderRepo.GetOrderById(orderId);
                if (order == null)
                {
                    return NotFound("Order not found");
                }

                // Simulate seller confirmation
                var oldStatus = order.Status;
                order.Status = request.NewStatus;
                var updatedOrder = _orderRepo.UpdateOrder(order);

                // Apply seller confirmation logic
                bool productStatusUpdated = false;
                if (order.SellerId == request.SellerId && 
                    (request.NewStatus == "Confirmed" || request.NewStatus == "Completed") &&
                    oldStatus != request.NewStatus &&
                    order.ProductId.HasValue)
                {
                    var product = _productRepo.GetProductById(order.ProductId.Value);
                    if (product != null && product.Status == "Reserved")
                    {
                        product.Status = "Sold";
                        _productRepo.UpdateProduct(product);
                        productStatusUpdated = true;
                    }
                }

                var response = new
                {
                    OrderId = updatedOrder.OrderId,
                    OldStatus = oldStatus,
                    NewStatus = updatedOrder.Status,
                    SellerId = order.SellerId,
                    ProductId = order.ProductId,
                    ProductStatusUpdated = productStatusUpdated,
                    UpdatedDate = DateTime.Now
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Test seller confirm error: " + ex.Message);
            }
        }

        [HttpGet("buyer")]
        [Authorize(Policy = "MemberOnly")]
        public ActionResult GetMyPurchases()
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var orders = _orderRepo.GetOrdersByBuyerId(userId);

                // ✅ FIX: Group by ProductId and keep only the most recent order for each product
                // Priority: Completed orders first, then by CreatedDate descending
                var uniqueOrders = orders
                    .GroupBy(o => o.ProductId)
                    .Select(g => g
                        .OrderByDescending(o => o.Status == "Completed" ? 1 : 0) // Completed first
                        .ThenByDescending(o => o.CompletedDate ?? o.CreatedDate) // Most recent first
                        .First())
                    .OrderByDescending(o => o.CompletedDate ?? o.CreatedDate) // ✅ Sort final list by date (newest first)
                    .ToList();

                var response = uniqueOrders.Select(o => new
                {
                    o.OrderId,
                    o.BuyerId,
                    o.TotalAmount,
                    o.DepositAmount,
                    o.Status,
                    OrderStatus = o.Status, // Alias for frontend compatibility
                    o.DepositStatus,
                    o.FinalPaymentStatus,
                    o.CreatedDate,
                    o.CompletedDate,
                    o.CancellationReason,
                    o.CancelledDate,
                    PurchaseDate = o.CompletedDate ?? o.CreatedDate, // Use CompletedDate if available, otherwise CreatedDate
                    SellerName = o.Seller?.FullName ?? "N/A",
                    SellerId = o.SellerId,
                    Product = o.Product != null ? new
                    {
                        ProductId = (int?)o.Product.ProductId,
                        Title = o.Product.Title,
                        Price = o.Product.Price,
                        ProductType = o.Product.ProductType ?? string.Empty,
                        Status = o.Product.Status ?? (string?)null,
                        Brand = o.Product.Brand ?? string.Empty,
                        Model = o.Product.Model,
                        Condition = o.Product.Condition,
                        VehicleType = o.Product.VehicleType,
                        LicensePlate = o.Product.LicensePlate,
                        ImageData = o.Product.ProductImages?.FirstOrDefault()?.ImageData // Get first product image
                    } : new
                    {
                        ProductId = (int?)null,
                        Title = "Sản phẩm không tìm thấy",
                        Price = o.TotalAmount, // Use order amount as fallback
                        ProductType = string.Empty,
                        Status = (string?)"Unknown",
                        Brand = string.Empty,
                        Model = (string?)null,
                        Condition = (string?)null,
                        VehicleType = (string?)null,
                        LicensePlate = (string?)null,
                        ImageData = (string?)null
                    },
                    // Additional debug info
                    DebugInfo = new
                    {
                        HasProduct = o.Product != null,
                        ProductId = o.ProductId,
                        OrderStatus = o.Status,
                        IsCompleted = o.Status == "Completed"
                    }
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("seller")]
        [Authorize(Policy = "MemberOnly")]
        public ActionResult GetMySales()
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var orders = _orderRepo.GetOrdersBySellerId(userId);

                var response = orders.Select(o => new
                {
                    o.OrderId,
                    ProductId = o.ProductId, // ✅ THÊM
                    o.TotalAmount,
                    o.DepositAmount, // ✅ THÊM
                    o.Status,
                    OrderStatus = o.Status, // ✅ THÊM (alias)
                    o.DepositStatus, // ✅ THÊM
                    o.PayoutStatus,
                    o.CreatedDate,
                    o.CompletedDate, // ✅ THÊM
                    o.CancellationReason,
                    o.CancelledDate,
                    BuyerName = o.Buyer?.FullName,
                    BuyerId = o.BuyerId, // ✅ THÊM
                    Product = o.Product != null ? new
                    {
                        ProductId = o.Product.ProductId, // ✅ THÊM
                        o.Product.Title,
                        o.Product.Price,
                        Status = o.Product.Status, // ✅ QUAN TRỌNG!
                        ProductType = o.Product.ProductType ?? string.Empty,
                        Brand = o.Product.Brand ?? string.Empty,
                        Model = o.Product.Model,
                        Condition = o.Product.Condition,
                        VehicleType = o.Product.VehicleType,
                        LicensePlate = o.Product.LicensePlate,
                        ImageData = o.Product.ProductImages?.FirstOrDefault()?.ImageData // ✅ THÊM
                    } : null
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("debug-purchases")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult DebugPurchases()
        {
            try
            {
                var orders = _orderRepo.GetAllOrders();
                
                var problematicOrders = orders.Where(o => 
                    o.Status == "Completed" && 
                    (o.Product == null || o.CompletedDate == null)
                ).ToList();

                var debugInfo = new
                {
                    TotalOrders = orders.Count,
                    CompletedOrders = orders.Count(o => o.Status == "Completed"),
                    ProblematicOrders = problematicOrders.Count,
                    ProblematicDetails = problematicOrders.Select(o => new
                    {
                        o.OrderId,
                        o.BuyerId,
                        o.SellerId,
                        o.ProductId,
                        o.Status,
                        o.CreatedDate,
                        o.CompletedDate,
                        HasProduct = o.Product != null,
                        ProductTitle = o.Product?.Title ?? "NULL",
                        ProductStatus = o.Product?.Status ?? "NULL"
                    }).ToList(),
                    AllCompletedOrders = orders.Where(o => o.Status == "Completed").Select(o => new
                    {
                        o.OrderId,
                        o.ProductId,
                        o.Status,
                        o.CompletedDate,
                        ProductExists = o.Product != null,
                        ProductTitle = o.Product?.Title
                    }).ToList()
                };

                return Ok(debugInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Debug error: " + ex.Message);
            }
        }

        [HttpPost("fix-completed-orders")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult FixCompletedOrders()
        {
            try
            {
                var orders = _orderRepo.GetAllOrders();
                var fixedCount = 0;
                var errors = new List<string>();

                foreach (var order in orders.Where(o => o.Status == "Completed"))
                {
                    try
                    {
                        // Fix missing CompletedDate
                        if (!order.CompletedDate.HasValue)
                        {
                            order.CompletedDate = order.CreatedDate ?? DateTime.Now;
                            _orderRepo.UpdateOrder(order);
                            fixedCount++;
                        }

                        // Check if product exists and is properly linked
                        if (order.ProductId.HasValue && order.Product == null)
                        {
                            var product = _productRepo.GetProductById(order.ProductId.Value);
                            if (product == null)
                            {
                                errors.Add($"Order {order.OrderId}: Product {order.ProductId} not found");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error fixing order {order.OrderId}: {ex.Message}");
                    }
                }

                return Ok(new
                {
                    message = "Fix completed orders process finished",
                    fixedCount = fixedCount,
                    totalCompletedOrders = orders.Count(o => o.Status == "Completed"),
                    errors = errors,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Fix error: " + ex.Message);
            }
        }

		[HttpPost("{id}/admin-reject")]
		[Authorize(Policy = "AdminOnly")]
		public ActionResult AdminRejectOrder(int id, [FromBody] AdminRejectOrderRequest request)
		{
			try
			{
				if (request == null || string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length < 3)
					return BadRequest("Reason is required (min 3 characters).");

				var order = _orderRepo.GetOrderById(id);
				if (order == null) return NotFound("Order not found.");

				if (string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase))
					return BadRequest("Cannot reject a completed order.");

				order.Status = "Cancelled";
				order.CompletedDate = null;
				order.CancellationReason = request.Reason;
				order.CancelledDate = DateTime.Now;

				var updated = _orderRepo.UpdateOrder(order);

				if (order.ProductId.HasValue)
				{
					var product = _productRepo.GetProductById(order.ProductId.Value);
					if (product != null && string.Equals(product.Status, "Reserved", StringComparison.OrdinalIgnoreCase))
					{
						product.Status = "Active";
						_productRepo.UpdateProduct(product);
					}
				}

				return Ok(new
				{
					updated.OrderId,
					updated.Status,
					Reason = request.Reason,
					CancelledDate = updated.CancelledDate,
					Message = "Order rejected successfully."
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, "Internal server error: " + ex.Message);
			}
		}
    }
}

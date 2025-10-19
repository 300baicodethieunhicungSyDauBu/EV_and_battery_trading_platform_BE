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

                order.Status = request.Status;
                var updatedOrder = _orderRepo.UpdateOrder(order);

                var response = new
                {
                    updatedOrder.OrderId,
                    updatedOrder.Status,
                    UpdatedDate = DateTime.Now
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
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

                var response = orders.Select(o => new
                {
                    o.OrderId,
                    o.TotalAmount,
                    o.Status,
                    o.CreatedDate,
                    SellerName = o.Seller?.FullName,
                    Product = new
                    {
                        o.Product?.Title,
                        o.Product?.Price
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
                    o.TotalAmount,
                    o.Status,
                    o.PayoutStatus,
                    o.CreatedDate,
                    BuyerName = o.Buyer?.FullName,
                    Product = new
                    {
                        o.Product?.Title,
                        o.Product?.Price
                    }
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

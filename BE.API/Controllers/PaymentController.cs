using BE.API.DTOs.Request;
using BE.BOs.Models;
using BE.BOs.VnPayModels;
using BE.REPOs.Implementation;
using BE.REPOs.Interface;
using BE.REPOs.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepo _paymentRepo;
        private readonly IOrderRepo _orderRepo;
        private readonly IProductRepo _productRepo;
        private readonly IVnPayService _vnpayService;

        public PaymentController(IPaymentRepo paymentRepo, IOrderRepo orderRepo,IProductRepo productRepo, IVnPayService vnpayService)
        {
            _paymentRepo = paymentRepo;
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _vnpayService = vnpayService;
        }

        [HttpPost]
        [Authorize(Policy = "MemberOnly")]
        public IActionResult CreatePayment([FromBody] PaymentRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

                // Nếu client gửi 0 thì coi như null
                var orderId = request.OrderId > 0 ? request.OrderId : null;
                var productId = request.ProductId > 0 ? request.ProductId : null;

                // Xác định loại thanh toán
                string paymentType;
                if (orderId != null && productId == null)
                    paymentType = "Deposit"; // hoặc "FinalPayment" tùy luồng
                else if (productId != null && orderId == null)
                    paymentType = "Verification";
                else
                    return BadRequest("Either OrderId or ProductId must be provided, not both.");

                // Tạo bản ghi thanh toán
                var payment = new Payment
                {
                    OrderId = orderId,
                    ProductId = productId,
                    PayerId = userId,
                    PaymentType = paymentType,
                    Amount = request.Amount,
                    PaymentMethod = "VNPay",
                    Status = "Pending",
                    CreatedDate = DateTime.Now
                };

                var createdPayment = _paymentRepo.CreatePayment(payment);

                var paymentInfo = new PaymentInformationModel
                {
                    OrderType = "other",
                    Amount = createdPayment.Amount,
                    OrderDescription = $"Thanh toán {paymentType.ToLower()} - ID: {createdPayment.PaymentId}",
                    Name = createdPayment.PaymentId.ToString()
                };

                var paymentUrl = _vnpayService.CreatePaymentUrl(paymentInfo, HttpContext);

                return Ok(new
                {
                    PaymentId = createdPayment.PaymentId,
                    PaymentUrl = paymentUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult GetAllPayments()
        {
            try
            {
                var payments = _paymentRepo.GetAllPayments();
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "MemberOnly")]
        public IActionResult GetPaymentById(int id)
        {
            try
            {
                var payment = _paymentRepo.GetPaymentById(id);
                if (payment == null)
                    return NotFound($"Payment with ID {id} not found");
                
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var userRole = User.FindFirst("Role")?.Value ?? "";
                
                // Admin có thể xem tất cả, user chỉ xem payment của mình
                if (userRole != "Admin" && payment.PayerId != userId)
                    return Forbid("You can only view your own payments");
                
                return Ok(payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("my-payments")]
        [Authorize(Policy = "MemberOnly")]
        public IActionResult GetMyPayments()
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var payments = _paymentRepo.GetPaymentsByPayerId(userId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("payer/{payerId}")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult GetPaymentsByPayerId(int payerId)
        {
            try
            {
                var payments = _paymentRepo.GetPaymentsByPayerId(payerId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("order/{orderId}")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult GetPaymentsByOrderId(int orderId)
        {
            try
            {
                var payments = _paymentRepo.GetPaymentsByOrderId(orderId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("vnpay-return")]
        [AllowAnonymous]
        public IActionResult VnPayReturn([FromQuery] Dictionary<string, string> queryParams)
        {
            try
            {
                if (!queryParams.ContainsKey("vnp_TxnRef") || !queryParams.ContainsKey("vnp_SecureHash"))
                    return BadRequest("Invalid VNPay callback.");

                // 1️⃣ Validate chữ ký
                var isValidSignature = _vnpayService.ValidateSignature(queryParams);
                if (!isValidSignature)
                    return BadRequest("Invalid signature");

                // 2️⃣ Lấy PaymentId từ callback
                long paymentIdLong = long.Parse(queryParams["vnp_TxnRef"]);
                var payment = _paymentRepo.GetPaymentById((int)paymentIdLong);
                if (payment == null)
                    return NotFound("Payment not found");

                // 3️⃣ Lấy response code từ VNPay
                string responseCode = queryParams.ContainsKey("vnp_ResponseCode")
                    ? queryParams["vnp_ResponseCode"]
                    : "";

                if (responseCode != "00")
                {
                    payment.Status = "Failed";
                    _paymentRepo.UpdatePayment(payment);
                    return Ok(new { message = "Payment failed", paymentId = payment.PaymentId });
                }

                // 4️⃣ Nếu thành công
                payment.Status = "Success";
                _paymentRepo.UpdatePayment(payment);

                // 5️⃣ Xử lý nghiệp vụ theo loại thanh toán
                if (payment.PaymentType == "Deposit")
                {
                    var order = _orderRepo.GetOrderById(payment.OrderId.Value);
                    if (order != null)
                    {
                        order.DepositStatus = "Paid";
                        order.Status = "Deposited";
                        _orderRepo.UpdateOrder(order);
                    }
                }
                else if (payment.PaymentType == "FinalPayment")
                {
                    var order = _orderRepo.GetOrderById(payment.OrderId.Value);
                    if (order != null)
                    {
                        order.FinalPaymentStatus = "Paid";
                        order.Status = "Completed";
                        order.CompletedDate = DateTime.Now;
                        _orderRepo.UpdateOrder(order);
                    }
                }
                else if (payment.PaymentType == "Verification")
                {
                    if (payment.ProductId != null)
                    {
                        var product = _productRepo.GetProductById(payment.ProductId.Value);
                        if (product != null)
                        {
                            product.VerificationStatus = "Requested";
                            _productRepo.UpdateProduct(product);
                        }
                    }
                }

                // 6️⃣ Trả về kết quả
                return Ok(new
                {
                    message = "Payment success",
                    paymentId = payment.PaymentId,
                    type = payment.PaymentType,
                    status = payment.Status
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error processing VNPay return: " + ex.Message);
            }
        }

    }
}

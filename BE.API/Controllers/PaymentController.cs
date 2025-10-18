using BE.API.DTOs.Request;
using BE.BOs.Models;
using BE.BOs.VnPayModels;
using BE.REPOs.Interface;
using BE.REPOs.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // /api/payment
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepo _paymentRepo;
        private readonly IOrderRepo _orderRepo;
        private readonly IProductRepo _productRepo;
        private readonly IVnPayService _vnPay;

        public PaymentController(IPaymentRepo paymentRepo, IOrderRepo orderRepo, IProductRepo productRepo, IVnPayService vnPay)
        {
            _paymentRepo = paymentRepo;
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _vnPay = vnPay;
        }

        [HttpPost]
        [Authorize(Policy = "MemberOnly")]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request)
        {
            var userIdStr = User.FindFirst("UserId")?.Value ?? "0";
            if (!int.TryParse(userIdStr, out var userId) || userId <= 0) return Unauthorized("Invalid user");

            // Xác định loại thanh toán
            var paymentType = request.PaymentType?.Trim();
            if (string.IsNullOrEmpty(paymentType))
            {
                // suy ra nếu FE không gửi
                paymentType = (request.ProductId.HasValue && !request.OrderId.HasValue) ? "Verification"
                             : (request.OrderId.HasValue ? "Deposit" : "");
            }
            if (string.IsNullOrEmpty(paymentType)) return BadRequest("PaymentType is required.");
            if (request.Amount <= 0) return BadRequest("Amount must be > 0");
            if ((paymentType is "Deposit" or "FinalPayment") && !request.OrderId.HasValue)
                return BadRequest($"{paymentType} requires OrderId.");
            if (paymentType == "Verification" && !request.ProductId.HasValue)
                return BadRequest("Verification requires ProductId.");

            var payment = new Payment
            {
                OrderId = request.OrderId,
                ProductId = request.ProductId,
                PayerId = userId,
                PaymentType = paymentType,
                Amount = request.Amount,
                PaymentMethod = "VNPAY",
                Status = "Pending",
                CreatedDate = DateTime.Now
            };

            var created = await _paymentRepo.CreatePaymentAsync(payment);

            var info = new PaymentInformationModel
            {
                OrderType = "other",
                Amount = created.Amount,
                OrderDescription = $"Thanh toán {paymentType.ToLower()} - ID: {created.PaymentId}",
                Name = created.PaymentId.ToString() // dùng làm vnp_TxnRef
            };

            var paymentUrl = _vnPay.CreatePaymentUrl(info, HttpContext);
            return Ok(new { PaymentId = created.PaymentId, PaymentUrl = paymentUrl });
        }

        [HttpGet("vnpay-return")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayReturn([FromQuery] Dictionary<string, string> query)
        {
            if (query is null || !query.ContainsKey("vnp_TxnRef") || !query.ContainsKey("vnp_SecureHash"))
                return BadRequest("Invalid VNPay callback.");
            if (!_vnPay.ValidateSignature(query)) return BadRequest("Invalid signature");
            if (!int.TryParse(query["vnp_TxnRef"], out var paymentId)) return BadRequest("Invalid TxnRef");

            var payment = await _paymentRepo.GetPaymentForUpdateAsync(paymentId);
            if (payment is null) return NotFound("Payment not found");
            if (await _paymentRepo.HasSuccessfulPaymentAsync(paymentId))
                return Ok(new { message = "Payment already succeeded", paymentId });

            // Lấy response (option: dùng PaymentExecute để map đầy đủ)
            var resp = _vnPay.PaymentExecute(Request.Query);
            var responseCode = query.GetValueOrDefault("vnp_ResponseCode", "");

            payment.TransactionNo = query.GetValueOrDefault("vnp_TransactionNo", "");
            payment.BankCode = query.GetValueOrDefault("vnp_BankCode", "");
            payment.CardType = query.GetValueOrDefault("vnp_CardType", "");
            payment.TransactionStatus = query.GetValueOrDefault("vnp_TransactionStatus", "");
            payment.ResponseCode = responseCode;
            var payDateRaw = query.GetValueOrDefault("vnp_PayDate", "");
            if (!string.IsNullOrWhiteSpace(payDateRaw) &&
                DateTime.TryParseExact(payDateRaw, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var payDate))
            {
                payment.PayDate = payDate;
            }

            if (responseCode == "00" && resp.Success)
            {
                payment.Status = "Success";
                await _paymentRepo.UpdatePaymentAsync(payment);

                // Nghiệp vụ
                if (payment.PaymentType == "Deposit" && payment.OrderId.HasValue)
                {
                    var od = _orderRepo.GetOrderById(payment.OrderId.Value);
                    if (od != null) { od.DepositStatus = "Paid"; od.Status = "Deposited"; _orderRepo.UpdateOrder(od); }
                }
                else if (payment.PaymentType == "FinalPayment" && payment.OrderId.HasValue)
                {
                    var od = _orderRepo.GetOrderById(payment.OrderId.Value);
                    if (od != null) { od.FinalPaymentStatus = "Paid"; od.Status = "Completed"; od.CompletedDate = DateTime.Now; _orderRepo.UpdateOrder(od); }
                }
                else if (payment.PaymentType == "Verification" && payment.ProductId.HasValue)
                {
                    var p = _productRepo.GetProductById(payment.ProductId.Value);
                    if (p != null) { p.VerificationStatus = "Requested"; _productRepo.UpdateProduct(p); }
                }

                return Ok(new { message = "Payment success", paymentId = payment.PaymentId, type = payment.PaymentType });
            }
            else
            {
                payment.Status = "Failed";
                await _paymentRepo.UpdatePaymentAsync(payment);
                return Ok(new { message = "Payment failed", paymentId = payment.PaymentId, code = responseCode });
            }
        }

        [HttpPost("vnpay-ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayIpn([FromQuery] Dictionary<string, string> query)
        {
            if (query is null || !query.ContainsKey("vnp_TxnRef") || !query.ContainsKey("vnp_SecureHash")) return Content("Fail");
            if (!_vnPay.ValidateSignature(query)) return Content("Fail");
            if (!int.TryParse(query["vnp_TxnRef"], out var paymentId)) return Content("Fail");

            var payment = await _paymentRepo.GetPaymentForUpdateAsync(paymentId);
            if (payment is null) return Content("Fail");
            if (await _paymentRepo.HasSuccessfulPaymentAsync(paymentId)) return Content("OK");

            var responseCode = query.GetValueOrDefault("vnp_ResponseCode", "");
            payment.TransactionNo = query.GetValueOrDefault("vnp_TransactionNo", "");
            payment.ResponseCode = responseCode;
            payment.Status = responseCode == "00" ? "Success" : "Failed";
            await _paymentRepo.UpdatePaymentAsync(payment);

            return Content("OK");
        }
    }
}
    
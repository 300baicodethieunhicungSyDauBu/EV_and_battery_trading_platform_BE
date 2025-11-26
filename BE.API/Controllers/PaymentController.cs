using BE.API.DTOs.Request;
using BE.BOs.Models;
using BE.BOs.VnPayModels;
using BE.REPOs.Interface;
using BE.REPOs.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Globalization;
using System.Linq;


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
        private readonly IUserRepo _userRepo;
        private readonly IFeeSettingsRepo _feeSettingsRepo;
        private readonly ICreditHistoryRepo _creditHistoryRepo;
        private readonly EvandBatteryTradingPlatformContext _context;
        
        public PaymentController(
            IPaymentRepo paymentRepo, 
            IOrderRepo orderRepo, 
            IProductRepo productRepo,
            IVnPayService vnPay,
            IUserRepo userRepo,
            IFeeSettingsRepo feeSettingsRepo,
            ICreditHistoryRepo creditHistoryRepo,
            EvandBatteryTradingPlatformContext context)
        {
            _paymentRepo = paymentRepo;
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _vnPay = vnPay;
            _userRepo = userRepo;
            _feeSettingsRepo = feeSettingsRepo;
            _creditHistoryRepo = creditHistoryRepo;
            _context = context;
        }

        [HttpPost]
[Authorize(Policy = "MemberOnly")]
public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request)
{
    var userIdStr = User.FindFirst("UserId")?.Value ?? "0";
    if (!int.TryParse(userIdStr, out var userId) || userId <= 0) return Unauthorized("Invalid user");

    var paymentType = request.PaymentType?.Trim();
    if (string.IsNullOrEmpty(paymentType))
    {
        paymentType = (request.ProductId.HasValue && !request.OrderId.HasValue)
            ? "Verification"
            : (request.OrderId.HasValue ? "Deposit" : "");
    }
    if (string.IsNullOrEmpty(paymentType)) return BadRequest("PaymentType is required.");

    // ✅ VALIDATION THEO LOẠI PAYMENT
    if ((paymentType is "Deposit" or "FinalPayment") && (!request.OrderId.HasValue || request.OrderId <= 0))
        return BadRequest($"{paymentType} requires a valid OrderId.");
    if (paymentType == "Verification" && (!request.ProductId.HasValue || request.ProductId <= 0))
        return BadRequest("Verification requires a valid ProductId.");
    if (paymentType == "PostCredit" && (!request.PostCredits.HasValue || request.PostCredits <= 0))
        return BadRequest("PostCredits is required for PostCredit payment.");
    
    var postCreditsToAdd = paymentType == "PostCredit" ? request.PostCredits.Value : 0;

    // ✅ VALIDATE GIÁ TỪ DATABASE (Không tin client)
    if (paymentType == "PostCredit")
    {
        var packageFeeType = $"PostCredit_{request.PostCredits.Value}";
        var package = _feeSettingsRepo.GetSingleFeeByType(packageFeeType);
        
        if (package == null)
        {
            return BadRequest(new
            {
                error = "Invalid package",
                message = $"Package with {request.PostCredits} credits does not exist",
                availablePackages = new[] { 5, 10, 20, 50 }
            });
        }
        
        // ✅ GHI ĐÈ giá từ database (bảo mật)
        request.Amount = package.FeeValue;
    }

    // Validation số tiền VNPay
    const decimal VNPAY_MIN_AMOUNT = 5000m;
    const decimal VNPAY_MAX_AMOUNT = 999999999m;
    if (request.Amount <= 0) return BadRequest("Amount must be > 0");
    if (request.Amount < VNPAY_MIN_AMOUNT)
        return BadRequest($"Số tiền thanh toán phải tối thiểu {VNPAY_MIN_AMOUNT:N0} VND");
    if (request.Amount > VNPAY_MAX_AMOUNT)
        return BadRequest($"Số tiền thanh toán không được vượt quá {VNPAY_MAX_AMOUNT:N0} VND.");

    // Tạo payment
    var payment = new Payment
    {
        OrderId = request.OrderId > 0 ? request.OrderId : null,
        ProductId = request.ProductId > 0 ? request.ProductId : null,
        PayerId = userId,
        PaymentType = paymentType,
        Amount = request.Amount,
        PaymentMethod = "VNPAY",
        Status = "Pending",
        CreatedDate = DateTime.Now,
        PostCredits = postCreditsToAdd
    };

    HttpContext.Items["PostCredits"] = postCreditsToAdd;
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

        // ✅ Admin confirm endpoint - must be before generic {id} route
        [HttpPost("admin-confirm")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult AdminConfirmSale([FromBody] AdminAcceptWrapperRequest request)
        {
            try
            {
                // ✅ Authentication required: Chỉ admin đã đăng nhập mới có thể gọi API
                var userIdStr = User.FindFirst("UserId")?.Value ?? "0";
                if (!int.TryParse(userIdStr, out var userId) || userId <= 0)
                    return Unauthorized("Invalid user authentication");

                // ✅ Authorization check: Chỉ admin mới có thể xác nhận
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
                if (userRole != "1") // Assuming "1" is admin role
                    return Forbid("Only administrators can confirm sales");

                // Validate request
                if (request?.Request == null)
                    return BadRequest("Request data is required");

                if (request.Request.ProductId <= 0)
                    return BadRequest("Invalid product ID");

                // Get the product to verify status
                var product = _productRepo.GetProductById(request.Request.ProductId);
                if (product == null)
                    return NotFound("Product not found");

                // ✅ Status validation: Chỉ cho phép admin xác nhận sản phẩm có status "Reserved"
                if (product.Status != "Reserved")
                    return BadRequest(
                        $"Product must be in 'Reserved' status for admin confirmation. Current status: {product.Status}");

                // ✅ Tìm order theo ProductId, không phụ thuộc vào OrderStatus
                var allOrders = _orderRepo.GetAllOrders();
                var relatedOrder = allOrders.FirstOrDefault(o => o.ProductId == request.Request.ProductId);

                // ✅ Logging khi không tìm thấy order
                if (relatedOrder == null)
                {
                    return NotFound(new
                    {
                        message = "Order not found for this product",
                        productId = request.Request.ProductId
                    });
                }

                // ✅ Chỉ update nếu order chưa completed
                if (relatedOrder.Status?.ToLower() != "completed")
                {
                    relatedOrder.Status = "Completed";
                    relatedOrder.CompletedDate = DateTime.Now;
                    relatedOrder.FinalPaymentStatus = "Paid"; // Đánh dấu đã thanh toán đầy đủ
                    _orderRepo.UpdateOrder(relatedOrder);
                }
                else
                {
                    // Order đã completed rồi, không cần update lại nhưng vẫn tiếp tục update product
                }

                // ✅ Logic nghiệp vụ: Admin xác nhận và chuyển status từ "Reserved" → "Sold"
                product.Status = "Sold";

                // Update the product
                var updatedProduct = _productRepo.UpdateProduct(product);
                if (updatedProduct == null)
                    return StatusCode(500, "Failed to update product status");

                // ✅ Error handling: Xử lý các trường hợp lỗi một cách chi tiết
                return Ok(new
                {
                    message = "Admin confirmed sale successfully",
                    productId = updatedProduct.ProductId,
                    sellerId = updatedProduct.SellerId,
                    adminId = userId,
                    oldStatus = "Reserved",
                    newStatus = updatedProduct.Status,
                    orderId = relatedOrder.OrderId,
                    orderStatus = relatedOrder.Status,
                    orderCompletedDate = relatedOrder.CompletedDate,
                    orderUpdated = relatedOrder.Status?.ToLower() != "completed",
                    createdDate = updatedProduct.CreatedDate,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                // ✅ Error handling: Xử lý các trường hợp lỗi một cách chi tiết
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
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

                // Admin có thể xem tất cả, user chỉ xem payment của mình
                if (userRole != "1" && payment.PayerId != userId)
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
        public async Task<IActionResult> VnPayReturn([FromQuery] Dictionary<string, string> query)
        {
            // ❌ Trường hợp query không hợp lệ
            if (query is null || !query.ContainsKey("vnp_TxnRef") || !query.ContainsKey("vnp_SecureHash"))
                return Content("<script>alert('Invalid VNPay callback');window.close();</script>",
                    "text/html; charset=utf-8");

            if (!_vnPay.ValidateSignature(query))
                return Content("<script>alert('Invalid signature');window.close();</script>",
                    "text/html; charset=utf-8");

            if (!int.TryParse(query["vnp_TxnRef"], out var paymentId))
                return Content("<script>alert('Invalid TxnRef');window.close();</script>", "text/html; charset=utf-8");

            var payment = await _paymentRepo.GetPaymentForUpdateAsync(paymentId);
            if (payment is null)
                return Content("<script>alert('Payment not found');window.close();</script>",
                    "text/html; charset=utf-8");

            if (await _paymentRepo.HasSuccessfulPaymentAsync(paymentId))
            {
                string htmlAlready = $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <title>Đã thanh toán</title>
</head>
<body style='font-family:sans-serif;text-align:center;margin-top:80px;'>
    <h2>Thanh toán đã được ghi nhận trước đó!</h2>
    <p>Bạn có thể đóng cửa sổ này.</p>
    <script>
        if (window.opener) {{
            window.opener.postMessage({{
                status: 'success',
                paymentId: '{paymentId}',
                message: 'already-paid'
            }}, '*');
            window.close();
        }} else {{
            document.body.innerHTML += '<p>Vui lòng đóng tab này thủ công.</p>';
        }}
    </script>
</body>
</html>";
                return Content(htmlAlready, "text/html; charset=utf-8");
            }

            // Xử lý kết quả VNPay
            var resp = _vnPay.PaymentExecute(Request.Query);
            var responseCode = query.GetValueOrDefault("vnp_ResponseCode", "");

            payment.TransactionNo = query.GetValueOrDefault("vnp_TransactionNo", "");
            payment.BankCode = query.GetValueOrDefault("vnp_BankCode", "");
            payment.CardType = query.GetValueOrDefault("vnp_CardType", "");
            payment.TransactionStatus = query.GetValueOrDefault("vnp_TransactionStatus", "");
            payment.ResponseCode = responseCode;

            var payDateRaw = query.GetValueOrDefault("vnp_PayDate", "");
            if (!string.IsNullOrWhiteSpace(payDateRaw) &&
                DateTime.TryParseExact(payDateRaw, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var payDate))
            {
                payment.PayDate = payDate;
            }

            // Thành công
            if (responseCode == "00" && resp.Success)
            {
                payment.Status = "Success";
                await _paymentRepo.UpdatePaymentAsync(payment);

                try
                {
                    // Nghiệp vụ theo loại thanh toán
                    if (payment.PaymentType == "Deposit" && payment.OrderId.HasValue)
                    {
                        try
                        {
                            var od = _orderRepo.GetOrderById(payment.OrderId.Value);
                            if (od != null)
                            {
                                // Cập nhật Order status và deposit status
                                od.DepositStatus = "Paid";
                                od.Status = "Deposited";
                                var updatedOrder = _orderRepo.UpdateOrder(od);

                                // Cập nhật Product status
                                if (od.ProductId.HasValue)
                                {
                                    var product = _productRepo.GetProductById(od.ProductId.Value);
                                    if (product != null && product.Status == "Active")
                                    {
                                        product.Status = "Reserved";
                                        _productRepo.UpdateProduct(product);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log error nhưng không throw để không ảnh hưởng payment callback
                            System.Diagnostics.Debug.WriteLine($"Error updating order after deposit: {ex.Message}");
                        }
                    }
                    else if (payment.PaymentType == "FinalPayment" && payment.OrderId.HasValue)
                    {
                        var od = _orderRepo.GetOrderById(payment.OrderId.Value);
                        if (od != null)
                        {
                            od.FinalPaymentStatus = "Paid";
                            od.Status = "Completed";
                            od.CompletedDate = DateTime.Now;
                            _orderRepo.UpdateOrder(od);

                            if (od.ProductId.HasValue)
                            {
                                var product = _productRepo.GetProductById(od.ProductId.Value);
                                if (product != null && product.Status == "Reserved")
                                {
                                    product.Status = "Sold";
                                    _productRepo.UpdateProduct(product);
                                }
                            }
                        }
                    }
                    else if (payment.PaymentType == "Verification" && payment.ProductId.HasValue)
                    {
                        var p = _productRepo.GetProductById(payment.ProductId.Value);
                        if (p != null)
                        {
                            p.VerificationStatus = "Requested";
                            _productRepo.UpdateProduct(p);
                        }
                    }
                    else if (payment.PaymentType == "PostCredit" && payment.PostCredits.HasValue && payment.PostCredits.Value > 0)
                    {
                        try
                        {
                            // Lấy user và update credits
                            var user = _userRepo.GetUserById(payment.PayerId!.Value);
                            if (user == null)
                            {
                                Console.WriteLine($"[ERROR] User not found: {payment.PayerId}");
                                throw new Exception($"User not found: {payment.PayerId}");
                            }
                            
                            // Lưu credits trước khi thay đổi
                            var creditsBefore = user.PostCredits;
                            
                            // Cộng credits
                            user.PostCredits += payment.PostCredits.Value;
                            _userRepo.UpdateUser(user);
                            
                            Console.WriteLine($"[INFO] Updated user credits: {creditsBefore} -> {user.PostCredits}");
                            
                            // LOG CREDIT CHANGE
                            await _creditHistoryRepo.LogCreditChange(new CreditHistory
                            {
                                UserId = user.UserId,
                                PaymentId = payment.PaymentId,
                                ChangeType = "Purchase",
                                CreditsBefore = creditsBefore,
                                CreditsChanged = payment.PostCredits.Value,
                                CreditsAfter = user.PostCredits,
                                Reason = $"Đã mua gói credit: {payment.PostCredits} credits với giá {payment.Amount:N0} VND",
                                CreatedDate = DateTime.Now
                            });
                            
                            Console.WriteLine($"[SUCCESS] PostCredit completed for user {user.UserId}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] PostCredit processing failed:");
                            Console.WriteLine($"  Message: {ex.Message}");
                            Console.WriteLine($"  Inner: {ex.InnerException?.Message}");
                            Console.WriteLine($"  Stack: {ex.StackTrace}");
                            // Không throw để không crash callback, chỉ log
                        }
                    }

                    // HTML trả về cho tab VNPay
                    string htmlSuccess = $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <title>Thanh toán thành công</title>
</head>
<body style='font-family:sans-serif;text-align:center;margin-top:80px;'>
    <h2>✅ Thanh toán thành công!</h2>
    <p>Bạn có thể đóng cửa sổ này.</p>
    <script>
        try {{
            if (window.opener) {{
                window.opener.postMessage({{
                    status: 'success',
                    paymentId: '{payment.PaymentId}',
                    type: '{payment.PaymentType}'
                }}, '*');
                window.close();
            }} else {{
                document.body.innerHTML += '<p>Vui lòng đóng tab này thủ công.</p>';
            }}
        }} catch (e) {{
            document.body.innerHTML += '<p>Không thể tự đóng cửa sổ, vui lòng đóng thủ công.</p>';
        }}
    </script>
</body>
</html>";

                    return Content(htmlSuccess, "text/html; charset=utf-8");
                }
                catch (Exception ex)
                {
                    // Log error chi tiết
                    Console.WriteLine($"[ERROR] Payment processing failed:");
                    Console.WriteLine($"  PaymentId: {payment.PaymentId}");
                    Console.WriteLine($"  PaymentType: {payment.PaymentType}");
                    Console.WriteLine($"  Error: {ex.Message}");
                    Console.WriteLine($"  StackTrace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"  InnerException: {ex.InnerException.Message}");
                    }
                    
                    return Content($"<script>alert('Payment processing failed: {ex.Message.Replace("'", "\\'")}');window.close();</script>", 
                                   "text/html; charset=utf-8");
                }
            }

            // ❌ Thất bại
            payment.Status = "Failed";
            await _paymentRepo.UpdatePaymentAsync(payment);

            string htmlFail = $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <title>Thanh toán thất bại</title>
</head>
<body style='font-family:sans-serif;text-align:center;margin-top:80px;'>
    <h2>❌ Thanh toán thất bại!</h2>
    <p>Mã lỗi: {responseCode}</p>
    <script>
        if (window.opener) {{
            window.opener.postMessage({{
                status: 'failed',
                paymentId: '{payment.PaymentId}',
                code: '{responseCode}'
            }}, '*');
            window.close();
        }} else {{
            document.body.innerHTML += '<p>Vui lòng đóng tab này thủ công.</p>';
        }}
    </script>
</body>
</html>";
            return Content(htmlFail, "text/html; charset=utf-8");
        }



        [HttpPost("vnpay-ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> VnPayIpn([FromQuery] Dictionary<string, string> query)
        {
            if (query is null || !query.ContainsKey("vnp_TxnRef") || !query.ContainsKey("vnp_SecureHash"))
                return Content("Fail");
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

        [HttpPost("seller-confirm")]
        [Authorize(Policy = "MemberOnly")]
        public IActionResult SellerConfirmSale([FromBody] SellerConfirmWrapperRequest request)
        {
            try
            {
                // ✅ Authentication required: Chỉ user đã đăng nhập mới có thể gọi API
                var userIdStr = User.FindFirst("UserId")?.Value ?? "0";
                if (!int.TryParse(userIdStr, out var userId) || userId <= 0)
                    return Unauthorized("Invalid user authentication");

                // Validate request
                if (request?.Request == null)
                    return BadRequest("Request data is required");

                if (request.Request.ProductId <= 0)
                    return BadRequest("Invalid product ID");

                // Get the product to verify ownership and status
                var product = _productRepo.GetProductById(request.Request.ProductId);
                if (product == null)
                    return NotFound("Product not found");

                // ✅ Authorization check: Chỉ owner của sản phẩm mới có thể xác nhận bán
                if (product.SellerId != userId)
                    return Forbid("You can only confirm sales for your own products");

                // ✅ Status validation: Chỉ cho phép xác nhận bán sản phẩm có status "Reserved"
                if (product.Status != "Reserved")
                    return BadRequest(
                        $"Product must be in 'Reserved' status to confirm sale. Current status: {product.Status}");

                // ✅ Logic nghiệp vụ: Cập nhật status từ "Reserved" → "Sold"
                product.Status = "Sold";

                // Update the product
                var updatedProduct = _productRepo.UpdateProduct(product);
                if (updatedProduct == null)
                    return StatusCode(500, "Failed to update product status");

                // ✅ Error handling: Xử lý các trường hợp lỗi một cách chi tiết
                return Ok(new
                {
                    message = "Sale confirmed successfully",
                    productId = updatedProduct.ProductId,
                    sellerId = updatedProduct.SellerId,
                    oldStatus = "Reserved",
                    newStatus = updatedProduct.Status,
                    createdDate = updatedProduct.CreatedDate,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                // ✅ Error handling: Xử lý các trường hợp lỗi một cách chi tiết
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("admin-accept")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult AdminAcceptSale([FromBody] AdminAcceptWrapperRequest request)
        {
            try
            {
                // ✅ Authentication required: Chỉ admin đã đăng nhập mới có thể gọi API
                var userIdStr = User.FindFirst("UserId")?.Value ?? "0";
                if (!int.TryParse(userIdStr, out var userId) || userId <= 0)
                    return Unauthorized("Invalid user authentication");

                // ✅ Authorization check: Chỉ admin mới có thể xác nhận
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
                if (userRole != "1") // Assuming "1" is admin role
                    return Forbid("Only administrators can accept sales");

                // Validate request
                if (request?.Request == null)
                    return BadRequest("Request data is required");

                if (request.Request.ProductId <= 0)
                    return BadRequest("Invalid product ID");

                // Get the product to verify status
                var product = _productRepo.GetProductById(request.Request.ProductId);
                if (product == null)
                    return NotFound("Product not found");

                // ✅ Status validation: Chỉ cho phép admin xác nhận sản phẩm có status "Reserved"
                if (product.Status != "Reserved")
                    return BadRequest(
                        $"Product must be in 'Reserved' status for admin acceptance. Current status: {product.Status}");

                // ✅ Logic nghiệp vụ: Admin xác nhận và chuyển status từ "Reserved" → "Sold"
                product.Status = "Sold";

                // Update the product
                var updatedProduct = _productRepo.UpdateProduct(product);
                if (updatedProduct == null)
                    return StatusCode(500, "Failed to update product status");

                // ✅ Error handling: Xử lý các trường hợp lỗi một cách chi tiết
                return Ok(new
                {
                    message = "Admin accepted sale successfully",
                    productId = updatedProduct.ProductId,
                    sellerId = updatedProduct.SellerId,
                    adminId = userId,
                    oldStatus = "Reserved",
                    newStatus = updatedProduct.Status,
                    createdDate = updatedProduct.CreatedDate,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                // ✅ Error handling: Xử lý các trường hợp lỗi một cách chi tiết
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("admin-accept-test")]
        [AllowAnonymous]
        public IActionResult AdminAcceptTest([FromBody] AdminAcceptWrapperRequest request)
        {
            try
            {
                return Ok(new
                {
                    message = "Admin accept test endpoint working",
                    receivedRequest = request,
                    timestamp = DateTime.Now,
                    debug = new
                    {
                        hasRequest = request != null,
                        hasRequestData = request?.Request != null,
                        productId = request?.Request?.ProductId
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Test error: {ex.Message}");
            }
        }

        [HttpGet("admin-accept-debug")]
        [AllowAnonymous]
        public IActionResult AdminAcceptDebug()
        {
            try
            {
                return Ok(new
                {
                    message = "Admin accept debug endpoint working",
                    timestamp = DateTime.Now,
                    availableEndpoints = new[]
                    {
                        "POST /api/Payment/admin-accept",
                        "POST /api/Payment/admin-accept-test",
                        "GET /api/Payment/admin-accept-debug"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Debug error: {ex.Message}");
            }
        }

        [HttpPost("admin-accept-auth-test")]
        [Authorize]
        public IActionResult AdminAcceptAuthTest([FromBody] AdminAcceptWrapperRequest request)
        {
            try
            {
                var userIdStr = User.FindFirst("UserId")?.Value ?? "0";
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "";

                return Ok(new
                {
                    message = "Admin accept auth test endpoint working",
                    authentication = new
                    {
                        userId = userIdStr,
                        userRole = userRole,
                        userName = userName,
                        isAdmin = userRole == "1"
                    },
                    receivedRequest = request,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Auth test error: {ex.Message}");
            }
        }

        // ✅ NEW ENDPOINTS FOR CREDIT SYSTEM

        /// <summary>
        /// Get available PostCredit packages
        /// </summary>
        [HttpGet("packages")]
        [AllowAnonymous]
        public ActionResult GetPostCreditPackages()
        {
            try
            {
                var feeSettings = _feeSettingsRepo.GetAllFeeSettings()
                    .Where(f => f.FeeType != null && f.FeeType.StartsWith("PostCredit_") && f.IsActive == true)
                    .ToList();

                if (!feeSettings.Any())
                {
                    return NotFound("No PostCredit pvailable");
                }

                var packages = feeSettings.Select(f =>
                {
                    var creditsStr = f.FeeType!.Replace("PostCredit_", "");
                    if (!int.TryParse(creditsStr, out var credits))
                        return null;

                    var pricePerCredit = f.FeeValue / credits;
                    var basePrice = 10000m; // Giá gốc 10,000 VND/credit
                    var discountPercent = (int)Math.Round((1 - (pricePerCredit / basePrice)) * 100);

                    return new PostCreditPackage
                    {
                        PackageId = f.FeeType,
                        Credits = credits,
                        Price = f.FeeValue,
                        PricePerCredit = pricePerCredit,
                        DiscountPercent = discountPercent > 0 ? discountPercent : 0,
                        IsPopular = credits == 10, // Gói 10 credits là phổ biến nhất
                        Description = f.Description
                    };
                })
                .Where(p => p != null)
                .OrderBy(p => p!.Credits)
                .ToList();

                return Ok(packages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get credit history for current user
        /// </summary>
        [HttpGet("credits/history")]
        [Authorize(Policy = "MemberOnly")]
        public ActionResult GetMyCreditHistory()
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var user = _userRepo.GetUserById(userId);
                
                if (user == null)
                    return NotFound("User not found");

                var history = _creditHistoryRepo.GetUserCreditHistory(userId);

                var response = history.Select(h => new
                {
                    h.HistoryId,
                    h.ChangeType,
                    h.CreditsBefore,
                    h.CreditsChanged,
                    h.CreditsAfter,
                    h.Reason,
                    h.CreatedDate,
                    RelatedPayment = h.PaymentId,
                    RelatedProduct = h.ProductId.HasValue ? new
                    {
                        ProductId = h.ProductId,
                        Title = h.Product?.Title
                    } : null,
                    AdminName = h.CreatedByUser?.FullName
                }).ToList();

                return Ok(new
                {
                    CurrentCredits = user.PostCredits,
                    TotalPurchased = history.Where(h => h.ChangeType == "Purchase").Sum(h => h.CreditsChanged),
                    TotalUsed = Math.Abs(history.Where(h => h.ChangeType == "Use").Sum(h => h.CreditsChanged)),
                    History = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Admin: Adjust user credits manually
        /// </summary>
        [HttpPost("admin/credits/adjust")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> AdminAdjustCredits([FromBody] AdminCreditAdjustmentRequest request)
        {
            try
            {
                // Validate
                if (request.UserId <= 0)
                    return BadRequest("Invalid user ID");
                
                if (request.CreditsChange == 0)
                    return BadRequest("Credits change cannot be zero");
                
                if (string.IsNullOrWhiteSpace(request.Reason))
                    return BadRequest("Reason is required");
                
                // Get admin info
                var adminId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                var admin = _userRepo.GetUserById(adminId);
                
                if (admin == null)
                    return Unauthorized("Admin not found");
                
                // Get target user
                var user = _userRepo.GetUserById(request.UserId);
                if (user == null)
                    return NotFound("User not found");
                
                // Check if result would be negative
                var newCredits = user.PostCredits + request.CreditsChange;
                if (newCredits < 0)
                    return BadRequest($"Cannot subtract {Math.Abs(request.CreditsChange)} credits. User only has {user.PostCredits} credits.");
                
                // ✅ START TRANSACTION
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    var creditsBefore = user.PostCredits;
                    
                    // Update user credits
                    user.PostCredits = newCredits;
                    _userRepo.UpdateUser(user);
                    
                    // Log the change
                    await _creditHistoryRepo.LogCreditChange(new CreditHistory
                    {
                        UserId = user.UserId,
                        ChangeType = "AdminAdjust",
                        CreditsBefore = creditsBefore,
                        CreditsChanged = request.CreditsChange,
                        CreditsAfter = user.PostCredits,
                        Reason = $"[{request.AdjustmentType}] {request.Reason} (bởi {admin.FullName})",
                        CreatedBy = adminId,
                        CreatedDate = DateTime.Now
                    });
                    
                    await transaction.CommitAsync();
                    
                    return Ok(new
                    {
                        Message = "Credits adjusted successfully",
                        UserId = user.UserId,
                        UserEmail = user.Email,
                        CreditsBefore = creditsBefore,
                        CreditsChanged = request.CreditsChange,
                        CreditsAfter = user.PostCredits,
                        AdjustedBy = admin.FullName,
                        Reason = request.Reason,
                        Timestamp = DateTime.Now
                    });
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to adjust credits: {ex.Message}");
            }
        }

        /// <summary>
        /// Admin: Get all credit history with filters
        /// </summary>
        [HttpGet("admin/credits/history")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult AdminGetAllCreditHistory(
            [FromQuery] int? userId = null,
            [FromQuery] string? changeType = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var query = _creditHistoryRepo.GetAllCreditHistory().AsQueryable();
                
                if (userId.HasValue)
                    query = query.Where(h => h.UserId == userId.Value);
                
                if (!string.IsNullOrEmpty(changeType))
                    query = query.Where(h => h.ChangeType == changeType);
                
                var total = query.Count();
                var history = query
                    .OrderByDescending(h => h.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                var response = history.Select(h => new
                {
                    h.HistoryId,
                    h.UserId,
                    UserEmail = h.User?.Email,
                    UserName = h.User?.FullName,
                    h.ChangeType,
                    h.CreditsBefore,
                    h.CreditsChanged,
                    h.CreditsAfter,
                    h.Reason,
                    h.CreatedBy,
                    AdminName = h.CreatedByUser?.FullName,
                    h.CreatedDate,
                    RelatedPayment = h.PaymentId,
                    RelatedProduct = h.ProductId
                }).ToList();
                
                return Ok(new
                {
                    TotalRecords = total,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                    History = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

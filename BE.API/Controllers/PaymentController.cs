using BE.API.DTOs.Request;
using BE.BOs.Models;
using BE.BOs.VnPayModels;
using BE.REPOs.Interface;
using BE.REPOs.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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

        public PaymentController(IPaymentRepo paymentRepo, IOrderRepo orderRepo, IProductRepo productRepo,
            IVnPayService vnPay)
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
                paymentType = (request.ProductId.HasValue && !request.OrderId.HasValue)
                    ? "Verification"
                    : (request.OrderId.HasValue ? "Deposit" : "");
            }

            if (string.IsNullOrEmpty(paymentType)) return BadRequest("PaymentType is required.");
            if (request.Amount <= 0) return BadRequest("Amount must be > 0");
            if ((paymentType is "Deposit" or "FinalPayment") && (!request.OrderId.HasValue || request.OrderId <= 0))
                return BadRequest($"{paymentType} requires a valid OrderId.");
            if (paymentType == "Verification" && (!request.ProductId.HasValue || request.ProductId <= 0))
                return BadRequest("Verification requires a valid ProductId.");

            var payment = new Payment
            {
                OrderId = request.OrderId > 0 ? request.OrderId : null,
                ProductId = request.ProductId > 0 ? request.ProductId : null,
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

        [HttpGet("test")]
        [Authorize]
        public IActionResult TestPaymentApi()
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;

                return Ok(new
                {
                    message = "Payment API is working!",
                    userId = userId,
                    userRole = userRole,
                    userName = userName,
                    timestamp = DateTime.Now,
                    canAccessAdminOnly = userRole == "1"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Test error: {ex.Message}");
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
            if (query is null || !query.ContainsKey("vnp_TxnRef") || !query.ContainsKey("vnp_SecureHash"))
                return Content("<script>alert('Invalid VNPay callback');window.close();</script>", "text/html");

            if (!_vnPay.ValidateSignature(query))
                return Content("<script>alert('Invalid signature');window.close();</script>", "text/html");

            if (!int.TryParse(query["vnp_TxnRef"], out var paymentId))
                return Content("<script>alert('Invalid TxnRef');window.close();</script>", "text/html");

            var payment = await _paymentRepo.GetPaymentForUpdateAsync(paymentId);
            if (payment is null)
                return Content("<script>alert('Payment not found');window.close();</script>", "text/html");

            if (await _paymentRepo.HasSuccessfulPaymentAsync(paymentId))
            {
                string htmlAlready = $@"
            <html><body>
            <script>
                if (window.opener) {{
                    window.opener.postMessage({{ status: 'success', paymentId: '{paymentId}', message: 'already-paid' }}, '*');
                    window.close();
                }} else {{
                    document.write('Payment already succeeded. You can close this tab.');
                }}
            </script></body></html>";
                return Content(htmlAlready, "text/html");
            }

            // ✅ Xử lý kết quả
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

            // ✅ Thanh toán thành công
            if (responseCode == "00" && resp.Success)
            {
                payment.Status = "Success";
                await _paymentRepo.UpdatePaymentAsync(payment);

                // Xử lý nghiệp vụ
                if (payment.PaymentType == "Deposit" && payment.OrderId.HasValue)
                {
                    var od = _orderRepo.GetOrderById(payment.OrderId.Value);
                    if (od != null)
                    {
                        od.DepositStatus = "Paid";
                        od.Status = "Deposited";
                        _orderRepo.UpdateOrder(od);

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

                // ✅ Trả HTML để đóng tab
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
                return new ContentResult
                {
                    Content = htmlSuccess,
                    ContentType = "text/html; charset=utf-8"
                };
            }
            else
            {
                payment.Status = "Failed";
                await _paymentRepo.UpdatePaymentAsync(payment);

                string htmlFail = $@"
            <html><body style='font-family:sans-serif;text-align:center;margin-top:80px;'>
                <h2>Thanh toán thất bại!</h2>
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
                        document.write('Vui lòng đóng tab này thủ công.');
                    }}
                </script>
            </body></html>";
                return Content(htmlFail, "text/html");
            }
        }


        [HttpPost("test-callback")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> TestCallback([FromBody] TestCallbackRequest request)
        {
            try
            {
                var payment = await _paymentRepo.GetPaymentForUpdateAsync(request.PaymentId);
                if (payment == null) return NotFound("Payment not found");

                // Simulate successful payment
                payment.Status = "Success";
                payment.ResponseCode = "00";
                payment.TransactionNo = "TEST_" + DateTime.Now.Ticks;
                payment.PayDate = DateTime.Now;
                await _paymentRepo.UpdatePaymentAsync(payment);

                // Apply business logic
                if (payment.PaymentType == "Deposit" && payment.OrderId.HasValue)
                {
                    var od = _orderRepo.GetOrderById(payment.OrderId.Value);
                    if (od != null)
                    {
                        od.DepositStatus = "Paid";
                        od.Status = "Deposited";
                        _orderRepo.UpdateOrder(od);

                        // Cập nhật Product status thành "Reserved" khi có deposit thành công
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
                else if (payment.PaymentType == "FinalPayment" && payment.OrderId.HasValue)
                {
                    var od = _orderRepo.GetOrderById(payment.OrderId.Value);
                    if (od != null)
                    {
                        od.FinalPaymentStatus = "Paid";
                        od.Status = "Completed";
                        od.CompletedDate = DateTime.Now;
                        _orderRepo.UpdateOrder(od);

                        // Cập nhật Product status thành "Sold" khi order hoàn thành
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

                return Ok(new
                {
                    message = "Test callback executed successfully",
                    paymentId = payment.PaymentId,
                    type = payment.PaymentType,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Test callback error: {ex.Message}");
            }
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
    }
}
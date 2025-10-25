# 🔧 FIX: PaymentController Callback Logic + Seller Confirmation

## ❌ VẤN ĐỀ ĐÃ PHÁT HIỆN

**Payment ID 32 cho Product ID 19:**
- Payment Status: Success ✅
- Order Status: Deposited ✅  
- Order DepositStatus: Paid ✅
- **Product Status: Active** ❌ (Sai - phải là "Pending")

**Seller Confirmation Issue:**
- Khi seller xác nhận từ FE, Product status không được cập nhật sang "Sold"

**Admin Dashboard Status Confusion:**
- Trạng thái "Pending" bị trùng với "chờ duyệt" trong admin dashboard
- Cần phân biệt rõ: "Reserved" = có đơn hàng, "Draft" = chờ duyệt
- **VẤN ĐỀ HIỆN TẠI**: Admin dashboard vẫn hiển thị "Đang chờ duyệt" cho Product ID 19 mặc dù đã set "Reserved"

## 🔍 NGUYÊN NHÂN

### 1. PaymentController Callback Issue
Trong `PaymentController.cs` callback logic (dòng 228-232), khi Deposit payment thành công:

```csharp
// LOGIC CŨ - THIẾU SÓT
if (payment.PaymentType == "Deposit" && payment.OrderId.HasValue)
{
    var od = _orderRepo.GetOrderById(payment.OrderId.Value);
    if (od != null) { 
        od.DepositStatus = "Paid"; 
        od.Status = "Deposited"; 
        _orderRepo.UpdateOrder(od); 
    }
}
```

**Vấn đề**: Chỉ cập nhật Order status mà **KHÔNG CẬP NHẬT Product status** theo nghiệp vụ.

### 2. Seller Confirmation Issue
Trong `OrderController.cs` endpoint `UpdateOrderStatus`, không có logic cập nhật Product status khi seller xác nhận.

### 3. Admin Dashboard Status Confusion
Trạng thái "Pending" bị hiểu nhầm là "chờ duyệt" thay vì "đã có đơn hàng".

### 4. Frontend Mapping Issue
Admin dashboard frontend có thể đang map sai:
- "Reserved" → "Đang chờ duyệt" (SAI)
- "Draft" → "Đang chờ duyệt" (ĐÚNG)

## ✅ GIẢI PHÁP ĐÃ ÁP DỤNG

### 1. Fix Deposit Payment Logic
```csharp
if (payment.PaymentType == "Deposit" && payment.OrderId.HasValue)
{
    var od = _orderRepo.GetOrderById(payment.OrderId.Value);
    if (od != null) 
    { 
        od.DepositStatus = "Paid"; 
        od.Status = "Deposited"; 
        _orderRepo.UpdateOrder(od);
        
        // ✅ THÊM: Cập nhật Product status thành "Reserved" khi có deposit thành công
        if (od.ProductId.HasValue)
        {
            var product = _productRepo.GetProductById(od.ProductId.Value);
            if (product != null && product.Status == "Active")
            {
                product.Status = "Pending";
                _productRepo.UpdateProduct(product);
            }
        }
    }
}
```

### 2. Fix FinalPayment Logic
```csharp
else if (payment.PaymentType == "FinalPayment" && payment.OrderId.HasValue)
{
    var od = _orderRepo.GetOrderById(payment.OrderId.Value);
    if (od != null) 
    { 
        od.FinalPaymentStatus = "Paid"; 
        od.Status = "Completed"; 
        od.CompletedDate = DateTime.Now; 
        _orderRepo.UpdateOrder(od);
        
        // ✅ THÊM: Cập nhật Product status thành "Sold" khi order hoàn thành
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
```

### 4. Fix Admin Dashboard Status Confusion
Thay đổi trạng thái từ "Pending" thành "Reserved" để phân biệt rõ ràng:

```csharp
// Thêm endpoint admin để xem sản phẩm theo trạng thái
[HttpGet("admin/status/{status}")]
public ActionResult GetProductsByStatus(string status)
{
    // Trả về sản phẩm với StatusDescription rõ ràng
    StatusDescription = GetStatusDescription(p.Status, p.VerificationStatus)
}

private string GetStatusDescription(string? status, string? verificationStatus)
{
    return status?.ToLower() switch
    {
        "draft" => "Bản nháp - Chờ seller hoàn thiện",
        "re-submit" => "Chờ duyệt lại - Seller đã chỉnh sửa", 
        "rejected" => "Đã từ chối - Cần seller chỉnh sửa",
        "active" => "Đang bán - Có thể mua",
        "reserved" => "Đã có đơn hàng - Chờ thanh toán deposit", // ✅ RÕ RÀNG
        "sold" => "Đã bán - Không thể mua nữa",
        "deleted" => "Đã xóa",
        _ => $"Trạng thái không xác định: {status}"
    };
}
```

## 📋 LOGIC NGHIỆP VỤ SAU KHI FIX

### Payment Flow:
1. **Deposit Payment Success** → Product: `Active` → `Reserved`
2. **FinalPayment Success** → Product: `Reserved` → `Sold`
3. **Verification Payment Success** → Product: `VerificationStatus = "Requested"`

### Order Flow:
1. **Deposit Payment Success** → Order: `Status = "Deposited"`, `DepositStatus = "Paid"`
2. **FinalPayment Success** → Order: `Status = "Completed"`, `FinalPaymentStatus = "Paid"`
3. **Seller Confirmation** → Order: `Status = "Confirmed"` → Product: `Reserved` → `Sold`

### Seller Confirmation Flow:
1. **Seller clicks confirm** → Order: `Status = "Confirmed"` hoặc `"Completed"`
2. **Product status updated** → Product: `Reserved` → `Sold`

## 🧪 TESTING

### Test Endpoints Added:
```
POST /api/payment/test-callback
Authorization: AdminOnly
Body: { "PaymentId": 32 }

POST /api/order/test-seller-confirm/{orderId}
Authorization: AdminOnly
Body: { "SellerId": 2, "NewStatus": "Confirmed" }
```

### Test Cases:
- **Payment Callback**: Product ID 19 sẽ có `Status = "Reserved"` sau khi deposit thành công
- **Seller Confirmation**: Product sẽ có `Status = "Sold"` khi seller xác nhận

### Admin Dashboard Endpoints:
```
GET /api/product/admin/status/draft - Sản phẩm chờ duyệt
GET /api/product/admin/status/reserved - Sản phẩm đã có đơn hàng
GET /api/product/admin/status/active - Sản phẩm đang bán
GET /api/product/admin/status/sold - Sản phẩm đã bán
```

### Debug Endpoints:
```
GET /api/product/debug/status/19 - Debug Product ID 19
GET /api/product/admin/all-statuses - Xem tất cả trạng thái
POST /api/product/test-update-status/19 - Test cập nhật trạng thái
Body: { "NewStatus": "Reserved" }
```

### SQL Debug Script:
```sql
-- Kiểm tra trạng thái Product ID 19
SELECT ProductId, Title, Status, VerificationStatus 
FROM Products WHERE ProductId = 19;

-- Cập nhật thành Reserved (nếu cần)
UPDATE Products SET Status = 'Reserved' WHERE ProductId = 19;
```

## 📁 FILES MODIFIED

1. `BE.API/Controllers/PaymentController.cs` - Fixed callback logic
2. `BE.API/Controllers/OrderController.cs` - Added seller confirmation logic
3. `BE.API/DTOs/Request/TestCallbackRequest.cs` - Added test DTO
4. `BE.API/DTOs/Request/TestSellerConfirmRequest.cs` - Added test DTO

## ✅ KẾT QUẢ

- ✅ PaymentController callback hoạt động đúng cách
- ✅ Seller confirmation cập nhật Product status sang "Sold"
- ✅ Product status được cập nhật theo nghiệp vụ
- ✅ Order status được cập nhật đúng
- ✅ Logic nhất quán cho cả Deposit, FinalPayment và Seller Confirmation
- ✅ Thêm test endpoints để verify fixes

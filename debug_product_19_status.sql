-- Script để kiểm tra và cập nhật trạng thái Product ID 19
-- Giải quyết vấn đề admin dashboard hiển thị sai trạng thái

-- 1. Kiểm tra trạng thái hiện tại của Product ID 19
SELECT 
    ProductId,
    Title,
    Status,
    VerificationStatus,
    SellerId,
    CreatedDate
FROM Products 
WHERE ProductId = 19;

-- 2. Kiểm tra Order liên quan đến Product ID 19
SELECT 
    o.OrderId,
    o.BuyerId,
    o.SellerId,
    o.ProductId,
    o.Status as OrderStatus,
    o.DepositStatus,
    o.FinalPaymentStatus,
    o.CreatedDate
FROM Orders o 
WHERE o.ProductId = 19;

-- 3. Kiểm tra Payment liên quan đến Product ID 19
SELECT 
    p.PaymentId,
    p.OrderId,
    p.ProductId,
    p.PaymentType,
    p.Amount,
    p.Status as PaymentStatus,
    p.TransactionNo,
    p.PayDate
FROM Payments p 
WHERE p.ProductId = 19 OR p.OrderId IN (
    SELECT OrderId FROM Orders WHERE ProductId = 19
);

-- 4. Cập nhật Product ID 19 thành trạng thái "Reserved" (nếu cần)
-- UPDATE Products 
-- SET Status = 'Reserved'
-- WHERE ProductId = 19;

-- 5. Kiểm tra lại sau khi cập nhật
-- SELECT 
--     ProductId,
--     Title,
--     Status,
--     VerificationStatus,
--     SellerId,
--     CreatedDate
-- FROM Products 
-- WHERE ProductId = 19;

-- 6. Kiểm tra tất cả sản phẩm có trạng thái "Reserved"
SELECT 
    ProductId,
    Title,
    Status,
    VerificationStatus,
    SellerId,
    CreatedDate
FROM Products 
WHERE Status = 'Reserved'
ORDER BY ProductId;

-- 7. Kiểm tra tất cả sản phẩm có trạng thái "Draft" (chờ duyệt)
SELECT 
    ProductId,
    Title,
    Status,
    VerificationStatus,
    SellerId,
    CreatedDate
FROM Products 
WHERE Status = 'Draft'
ORDER BY ProductId;

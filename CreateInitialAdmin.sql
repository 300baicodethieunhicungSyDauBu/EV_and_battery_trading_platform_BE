-- Script để tạo Admin đầu tiên và các role cần thiết
-- Chạy script này để giải quyết vấn đề "chicken and egg" với admin operations

USE [EvAndBatteryTradingPlatform]
GO

-- 1. Tạo UserRoles nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM UserRoles WHERE RoleId = 1)
BEGIN
    INSERT INTO UserRoles (RoleId, RoleName, CreatedDate)
    VALUES (1, 'Admin', GETDATE());
    PRINT 'Role Admin (ID: 1) đã được tạo thành công!'
END
ELSE
BEGIN
    PRINT 'Role Admin (ID: 1) đã tồn tại.'
END
GO

IF NOT EXISTS (SELECT * FROM UserRoles WHERE RoleId = 2)
BEGIN
    INSERT INTO UserRoles (RoleId, RoleName, CreatedDate)
    VALUES (2, 'Member', GETDATE());
    PRINT 'Role Member (ID: 2) đã được tạo thành công!'
END
ELSE
BEGIN
    PRINT 'Role Member (ID: 2) đã tồn tại.'
END
GO

-- 2. Tạo Admin đầu tiên
DECLARE @AdminEmail NVARCHAR(255) = 'admin@evtrading.com';
DECLARE @AdminPassword NVARCHAR(255) = 'Admin123!@#';
DECLARE @HashedPassword NVARCHAR(255);

-- Hash password bằng BCrypt (cần cài đặt SQL Server CLR hoặc dùng .NET để hash)
-- Tạm thời dùng password đơn giản, sẽ cần hash lại trong ứng dụng
SET @HashedPassword = '$2a$11$YourHashedPasswordHere'; -- Thay thế bằng hash thực tế

-- Kiểm tra xem admin đã tồn tại chưa
IF NOT EXISTS (SELECT * FROM Users WHERE Email = @AdminEmail)
BEGIN
    INSERT INTO Users (
        RoleId, 
        Email, 
        PasswordHash, 
        FullName, 
        Phone, 
        AccountStatus, 
        CreatedDate
    )
    VALUES (
        1, -- RoleId = 1 (Admin)
        @AdminEmail,
        @HashedPassword,
        'System Administrator',
        '0123456789',
        'Active',
        GETDATE()
    );
    
    PRINT 'Admin đầu tiên đã được tạo thành công!'
    PRINT 'Email: admin@evtrading.com'
    PRINT 'Password: Admin123!@#'
    PRINT 'VUI LÒNG ĐỔI PASSWORD SAU KHI ĐĂNG NHẬP LẦN ĐẦU!'
END
ELSE
BEGIN
    PRINT 'Admin đã tồn tại trong hệ thống.'
END
GO

-- 3. Hiển thị thông tin admin vừa tạo
SELECT 
    u.UserId,
    u.Email,
    u.FullName,
    u.RoleId,
    r.RoleName,
    u.AccountStatus,
    u.CreatedDate
FROM Users u
LEFT JOIN UserRoles r ON u.RoleId = r.RoleId
WHERE u.Email = 'admin@evtrading.com';
GO

-- 4. Hiển thị tất cả roles
SELECT 
    RoleId,
    RoleName,
    CreatedDate
FROM UserRoles
ORDER BY RoleId;
GO

-- 5. Đếm số lượng users theo role
SELECT 
    r.RoleName,
    COUNT(u.UserId) as UserCount
FROM UserRoles r
LEFT JOIN Users u ON r.RoleId = u.RoleId
GROUP BY r.RoleId, r.RoleName
ORDER BY r.RoleId;
GO

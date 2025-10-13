-- Script đơn giản để tạo Admin đầu tiên
-- Sử dụng password đã được hash sẵn bằng BCrypt

USE [EvAndBatteryTradingPlatform]
GO

-- 1. Tạo UserRoles
IF NOT EXISTS (SELECT * FROM UserRoles WHERE RoleId = 1)
BEGIN
    INSERT INTO UserRoles (RoleId, RoleName, CreatedDate) VALUES (1, 'Admin', GETDATE());
END

IF NOT EXISTS (SELECT * FROM UserRoles WHERE RoleId = 2)
BEGIN
    INSERT INTO UserRoles (RoleId, RoleName, CreatedDate) VALUES (2, 'Member', GETDATE());
END
GO

-- 2. Tạo Admin với password đã hash
-- Password: "admin123" đã được hash bằng BCrypt
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'admin@evtrading.com')
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
        1, -- Admin role
        'admin@evtrading.com',
        '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', -- Password: "admin123"
        'System Administrator',
        '0123456789',
        'Active',
        GETDATE()
    );
    
    PRINT '✅ Admin đã được tạo thành công!'
    PRINT '📧 Email: admin@evtrading.com'
    PRINT '🔑 Password: admin123'
END
ELSE
BEGIN
    PRINT '⚠️ Admin đã tồn tại trong hệ thống.'
END
GO

-- 3. Kiểm tra kết quả
SELECT 
    '👤 Admin Info' as Info,
    u.UserId,
    u.Email,
    u.FullName,
    r.RoleName,
    u.AccountStatus,
    u.CreatedDate
FROM Users u
LEFT JOIN UserRoles r ON u.RoleId = r.RoleId
WHERE u.Email = 'admin@evtrading.com';
GO

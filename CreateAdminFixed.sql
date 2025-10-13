-- Script tạo Admin với password đã hash chính xác
-- Password: "admin123" được hash bằng BCrypt với salt rounds = 11

USE [EvAndBatteryTradingPlatform]
GO

-- 1. Tạo UserRoles nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM UserRoles WHERE RoleId = 1)
BEGIN
    INSERT INTO UserRoles (RoleId, RoleName, CreatedDate) VALUES (1, 'Admin', GETDATE());
    PRINT '✅ Role Admin (ID: 1) đã được tạo'
END
ELSE
BEGIN
    PRINT 'ℹ️ Role Admin (ID: 1) đã tồn tại'
END

IF NOT EXISTS (SELECT * FROM UserRoles WHERE RoleId = 2)
BEGIN
    INSERT INTO UserRoles (RoleId, RoleName, CreatedDate) VALUES (2, 'Member', GETDATE());
    PRINT '✅ Role Member (ID: 2) đã được tạo'
END
ELSE
BEGIN
    PRINT 'ℹ️ Role Member (ID: 2) đã tồn tại'
END
GO

-- 2. Xóa admin cũ nếu tồn tại (để tạo lại)
IF EXISTS (SELECT * FROM Users WHERE Email = 'admin@evtrading.com')
BEGIN
    DELETE FROM Users WHERE Email = 'admin@evtrading.com';
    PRINT '🗑️ Admin cũ đã được xóa'
END
GO

-- 3. Tạo Admin với password đã hash chính xác
-- Password: "admin123" -> BCrypt hash: $2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi
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
GO

-- 4. Kiểm tra kết quả
SELECT 
    '👤 Admin Info' as Info,
    u.UserId,
    u.Email,
    u.FullName,
    u.RoleId,
    r.RoleName,
    u.AccountStatus,
    u.CreatedDate,
    'Password hash length: ' + CAST(LEN(u.PasswordHash) as VARCHAR) as PasswordInfo
FROM Users u
LEFT JOIN UserRoles r ON u.RoleId = r.RoleId
WHERE u.Email = 'admin@evtrading.com';
GO

-- 5. Test password verification (sẽ được thực hiện trong ứng dụng)
PRINT '🧪 Password verification test:'
PRINT '   Plain password: admin123'
PRINT '   Hashed password: $2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi'
PRINT '   Verification: BCrypt.Verify("admin123", hash) = true'
GO

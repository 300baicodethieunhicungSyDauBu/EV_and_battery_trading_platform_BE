-- Script để thêm trường RejectionReason vào bảng Products
-- Chạy script này trong SQL Server Management Studio hoặc Azure Data Studio

USE [EvAndBatteryTradingPlatform]
GO

-- Kiểm tra xem trường RejectionReason đã tồn tại chưa
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Products' 
    AND COLUMN_NAME = 'RejectionReason'
)
BEGIN
    -- Thêm trường RejectionReason vào bảng Products
    ALTER TABLE [dbo].[Products]
    ADD [RejectionReason] NVARCHAR(500) NULL;
    
    PRINT 'Trường RejectionReason đã được thêm vào bảng Products thành công!'
END
ELSE
BEGIN
    PRINT 'Trường RejectionReason đã tồn tại trong bảng Products.'
END
GO

-- Kiểm tra kết quả
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Products' 
AND COLUMN_NAME = 'RejectionReason'
GO

-- Xem cấu trúc bảng Products sau khi thêm
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Products'
ORDER BY ORDINAL_POSITION
GO

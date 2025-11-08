-- Quick fix: Add AccountStatusReason column to Users table
-- Copy and paste this into SQL Server Management Studio and run it

USE EVBATERRY_TRADING_PLATFORM;
GO

-- Add column if it doesn't exist
IF NOT EXISTS (
    SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Users' 
    AND COLUMN_NAME = 'AccountStatusReason'
)
BEGIN
    ALTER TABLE [dbo].[Users] ADD [AccountStatusReason] NVARCHAR(MAX) NULL;
    PRINT 'Column AccountStatusReason added successfully!';
END
ELSE
BEGIN
    PRINT 'Column AccountStatusReason already exists.';
END
GO


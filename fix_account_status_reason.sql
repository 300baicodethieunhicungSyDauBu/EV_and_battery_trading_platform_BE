-- Script to fix AccountStatusReason column issue
-- Run this script in your SQL Server database

USE EVBATERRY_TRADING_PLATFORM;
GO

-- Check if column exists (case-insensitive check)
IF NOT EXISTS (
    SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Users' 
    AND COLUMN_NAME = 'AccountStatusReason'
    COLLATE SQL_Latin1_General_CP1_CI_AS
)
BEGIN
    -- Try to drop column if it exists with different case
    IF EXISTS (
        SELECT * 
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'Users' 
        AND COLUMN_NAME LIKE '%AccountStatusReason%'
        COLLATE SQL_Latin1_General_CP1_CI_AS
    )
    BEGIN
        DECLARE @sql NVARCHAR(MAX);
        SELECT @sql = 'ALTER TABLE [dbo].[Users] DROP COLUMN [' + COLUMN_NAME + ']'
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'Users' 
        AND COLUMN_NAME LIKE '%AccountStatusReason%'
        COLLATE SQL_Latin1_General_CP1_CI_AS;
        
        EXEC sp_executesql @sql;
        PRINT 'Removed old column with incorrect name.';
    END
    
    -- Add the column with correct name
    ALTER TABLE [dbo].[Users] ADD [AccountStatusReason] NVARCHAR(MAX) NULL;
    PRINT 'Column AccountStatusReason has been added successfully.';
END
ELSE
BEGIN
    PRINT 'Column AccountStatusReason already exists.';
END
GO

-- Verify the column exists
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users' 
AND COLUMN_NAME LIKE '%AccountStatusReason%'
COLLATE SQL_Latin1_General_CP1_CI_AS;
GO

-- Show all columns in Users table for verification
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users'
ORDER BY ORDINAL_POSITION;
GO


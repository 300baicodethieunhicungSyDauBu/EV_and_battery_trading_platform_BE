-- =============================================
-- Database: EvAndBatteryTradingPlatform
-- =============================================
CREATE DATABASE EvAndBatteryTradingPlatform;
GO
USE EvAndBatteryTradingPlatform;
GO

-- =============================================
-- 1. Tables
-- =============================================

CREATE TABLE [UserRoles] (
  [RoleId] INT PRIMARY KEY IDENTITY(1,1),
  [RoleName] VARCHAR(50) UNIQUE NOT NULL,
  [CreatedDate] DATETIME2 DEFAULT (GETDATE())
);
GO

CREATE TABLE [Users] (
  [UserId] INT PRIMARY KEY IDENTITY(1,1),
  [RoleId] INT,
  [Email] VARCHAR(255) UNIQUE NOT NULL,
  [PasswordHash] VARCHAR(255) NOT NULL,
  [FullName] NVARCHAR(200),
  [Phone] VARCHAR(20),
  [Avatar] TEXT,
  [AccountStatus] VARCHAR(20) DEFAULT 'Active',
  [CreatedDate] DATETIME2 DEFAULT (GETDATE()),
  [ResetPasswordToken] NVARCHAR(MAX),
  [ResetPasswordTokenExpiry] DATETIME2
);
GO

CREATE TABLE [Products] (
  [ProductId] INT PRIMARY KEY IDENTITY(1,1),
  [SellerId] INT,
  [ProductType] VARCHAR(20) NOT NULL,
  [Title] NVARCHAR(255) NOT NULL,
  [Description] TEXT,
  [Price] DECIMAL(18,2) NOT NULL,
  [Brand] VARCHAR(100) NOT NULL,
  [Model] VARCHAR(150),
  [Condition] VARCHAR(50),
  [VehicleType] VARCHAR(50),
  [ManufactureYear] INT,
  [Mileage] INT,
  [Transmission] VARCHAR(50),
  [SeatCount] INT,
  [LicensePlate] NVARCHAR(20),
  [BatteryType] VARCHAR(50),
  [BatteryHealth] DECIMAL(5,2),
  [Capacity] DECIMAL(10,2),
  [Voltage] DECIMAL(8,2),
  [BMS] VARCHAR(100),
  [CellType] VARCHAR(50),
  [CycleCount] INT,
  [Status] VARCHAR(20) DEFAULT 'Draft',
  [VerificationStatus] VARCHAR(20) DEFAULT 'NotRequested',
  [CreatedDate] DATETIME2 DEFAULT (GETDATE())
);
GO

CREATE TABLE [ProductImages] (
  [ImageId] INT PRIMARY KEY IDENTITY(1,1),
  [ProductId] INT,
  [Name] VARCHAR(100),
  [ImageData] TEXT NOT NULL,
  [CreatedDate] DATETIME2 DEFAULT (GETDATE())
);
GO

CREATE TABLE [Orders] (
  [OrderId] INT PRIMARY KEY IDENTITY(1,1),
  [BuyerId] INT,
  [SellerId] INT,
  [ProductId] INT,
  [TotalAmount] DECIMAL(18,2) NOT NULL,
  [DepositAmount] DECIMAL(18,2) NOT NULL,
  [Status] VARCHAR(20) DEFAULT 'Pending',
  [DepositStatus] VARCHAR(20) DEFAULT 'Pending',
  [FinalPaymentStatus] VARCHAR(20) DEFAULT 'Pending',
  [FinalPaymentDueDate] DATETIME2,
  [PayoutAmount] DECIMAL(18,2),
  [PayoutStatus] VARCHAR(20) DEFAULT 'Pending',
  [CreatedDate] DATETIME2 DEFAULT (GETDATE()),
  [CompletedDate] DATETIME2
);
GO

CREATE TABLE [Payments] (
  [PaymentId] INT PRIMARY KEY IDENTITY(1,1),
  [OrderId] INT,
  [PayerId] INT,
  [PaymentType] VARCHAR(20),
  [Amount] DECIMAL(18,2) NOT NULL,
  [PaymentMethod] VARCHAR(50),
  [PaymentStatus] VARCHAR(20) DEFAULT 'Pending',
  [CreatedDate] DATETIME2 DEFAULT (GETDATE()),
  [TransactionNo] VARCHAR(100),
  [BankCode] VARCHAR(50),
  [BankTranNo] VARCHAR(100),
  [CardType] VARCHAR(50),
  [PayDate] DATETIME2,
  [ResponseCode] VARCHAR(10),
  [TransactionStatus] VARCHAR(10),
  [SecureHash] VARCHAR(512)
);
GO

CREATE TABLE [FeeSettings] (
  [FeeId] INT PRIMARY KEY IDENTITY(1,1),
  [FeeType] VARCHAR(50) NOT NULL,
  [FeeValue] DECIMAL(10,4) NOT NULL,
  [IsActive] BIT DEFAULT (1),
  [CreatedDate] DATETIME2 DEFAULT (GETDATE())
);
GO

CREATE TABLE [Favorites] (
  [FavoriteId] INT PRIMARY KEY IDENTITY(1,1),
  [UserId] INT,
  [ProductId] INT,
  [CreatedDate] DATETIME2 DEFAULT (GETDATE())
);
GO

CREATE TABLE [Reviews] (
  [ReviewId] INT PRIMARY KEY IDENTITY(1,1),
  [OrderId] INT,
  [ReviewerId] INT,
  [RevieweeId] INT,
  [Rating] INT,
  [Content] TEXT,
  [CreatedDate] DATETIME2 DEFAULT (GETDATE())
);
GO

CREATE TABLE [ReportedListings] (
  [ReportId] INT PRIMARY KEY IDENTITY(1,1),
  [ProductId] INT,
  [ReporterId] INT,
  [ReportType] VARCHAR(50),
  [ReportReason] VARCHAR(500) NOT NULL,
  [Status] VARCHAR(20) DEFAULT 'Pending',
  [CreatedDate] DATETIME2 DEFAULT (GETDATE())
);
GO

CREATE TABLE [Notifications] (
  [NotificationId] INT PRIMARY KEY IDENTITY(1,1),
  [UserId] INT,
  [NotificationType] VARCHAR(50),
  [Title] NVARCHAR(255) NOT NULL,
  [Content] TEXT,
  [CreatedDate] DATETIME2 DEFAULT (GETDATE())
);
GO

CREATE TABLE [Chats] (
  [ChatId] INT PRIMARY KEY IDENTITY(1,1),
  [User1Id] INT,
  [User2Id] INT,
  [CreatedDate] DATETIME2 DEFAULT (GETDATE())
);
GO

CREATE TABLE [Messages] (
  [MessageId] INT PRIMARY KEY IDENTITY(1,1),
  [ChatId] INT,
  [SenderId] INT,
  [Content] TEXT NOT NULL,
  [IsRead] BIT DEFAULT (0),
  [CreatedDate] DATETIME2 DEFAULT (GETDATE())
);
GO

-- =============================================
-- 2. Extended Properties (chỉ giữ phần quan trọng)
-- =============================================

EXEC sp_addextendedproperty 
@name = N'Column_Description', 
@value = 'Vehicle, Battery', 
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table', @level1name = 'Products',
@level2type = N'Column', @level2name = 'ProductType';
GO

EXEC sp_addextendedproperty 
@name = N'Column_Description', 
@value = 'Draft, Active, Sold, Expired', 
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table', @level1name = 'Products',
@level2type = N'Column', @level2name = 'Status';
GO

-- =============================================
-- 3. Foreign Keys
-- =============================================

ALTER TABLE [Users] ADD FOREIGN KEY ([RoleId]) REFERENCES [UserRoles] ([RoleId]);
ALTER TABLE [Products] ADD FOREIGN KEY ([SellerId]) REFERENCES [Users] ([UserId]);
ALTER TABLE [ProductImages] ADD FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]);
ALTER TABLE [Orders] ADD FOREIGN KEY ([BuyerId]) REFERENCES [Users] ([UserId]);
ALTER TABLE [Orders] ADD FOREIGN KEY ([SellerId]) REFERENCES [Users] ([UserId]);
ALTER TABLE [Orders] ADD FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]);
ALTER TABLE [Payments] ADD FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]);
ALTER TABLE [Payments] ADD FOREIGN KEY ([PayerId]) REFERENCES [Users] ([UserId]);
ALTER TABLE [Favorites] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]);
ALTER TABLE [Favorites] ADD FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]);
ALTER TABLE [Reviews] ADD FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId]);
ALTER TABLE [Reviews] ADD FOREIGN KEY ([ReviewerId]) REFERENCES [Users] ([UserId]);
ALTER TABLE [Reviews] ADD FOREIGN KEY ([RevieweeId]) REFERENCES [Users] ([UserId]);
ALTER TABLE [ReportedListings] ADD FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]);
ALTER TABLE [ReportedListings] ADD FOREIGN KEY ([ReporterId]) REFERENCES [Users] ([UserId]);
ALTER TABLE [Notifications] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]);
ALTER TABLE [Chats] ADD FOREIGN KEY ([User1Id]) REFERENCES [Users] ([UserId]);
ALTER TABLE [Chats] ADD FOREIGN KEY ([User2Id]) REFERENCES [Users] ([UserId]);
ALTER TABLE [Messages] ADD FOREIGN KEY ([ChatId]) REFERENCES [Chats] ([ChatId]);
ALTER TABLE [Messages] ADD FOREIGN KEY ([SenderId]) REFERENCES [Users] ([UserId]);
GO

-- =============================================
-- 4. Add OAuth Columns
-- =============================================
ALTER TABLE [Users]
ADD OAuthEmail VARCHAR(255) NULL,
    OAuthId VARCHAR(255) NULL,
    OAuthProvider VARCHAR(50) NULL;
GO

-- =============================================
-- 5. Seed Data
-- =============================================
INSERT INTO [UserRoles] ([RoleName]) VALUES ('Admin'), ('Member');
GO

INSERT INTO [Users] ([RoleId], [Email], [PasswordHash], [FullName], [Phone], [AccountStatus])
VALUES
(1, 'ptdtan43@gmail.com', 'admin123', N'System Administrator', '0902835570', 'Active'),
(2, 'tan@evtrade.com', 'member123', N'Phạm Từ Duy Tân', '0900000002', 'Active'),
(2, 'vinh@evtrade.com', 'member123', N'Nguyễn Vinh', '0900000003', 'Active');
GO

INSERT INTO [Products] (
  [SellerId], [ProductType], [Title], [Description], [Price], [Brand], [Model],
  [Condition], [VehicleType], [ManufactureYear], [Mileage], [Transmission],
  [SeatCount], [LicensePlate], [BatteryHealth], [BatteryType],
  [Capacity], [Voltage], [BMS], [CellType], [CycleCount], [Status]
)
VALUES
(2, 'Vehicle', N'VinFast VF e34 2022 - Sedan điện cao cấp',
 N'Xe VinFast VF e34 đời 2022, đã qua sử dụng, pin còn 85%, chạy 300km, nội thất cao cấp.',
 450000000, 'VinFast', 'VF e34', 'Good', 'Car', 2022, 15000, 'Automatic', 5, N'51H-123.45',
 85.5, 'CarBattery', 42.0, 400.0, 'VinFast BMS v2.0', 'LFP', 450, 'Active'),

(3, 'Battery', N'Pin xe máy điện Yamaha E01 - Dung lượng cao',
 N'Pin Yamaha E01 đã dùng 8 tháng, còn 92% dung lượng, kèm dây sạc và hộp bảo quản.',
 8500000, 'Yamaha', 'E01 Battery Pack', 'Excellent', NULL, NULL, NULL, NULL, NULL, NULL,
 92.0, 'MotorcycleBattery', 2.3, 48.0, 'Yamaha Smart BMS', 'NMC', 180, 'Active');
GO



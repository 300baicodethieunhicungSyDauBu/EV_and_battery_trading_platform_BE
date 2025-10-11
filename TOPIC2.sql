CREATE DATABASE Topic2;
GO
USE Topic2;
GO

-- ==========================
-- TABLE: UserRoles
-- ==========================
CREATE TABLE [UserRoles] (
  [RoleId] INT PRIMARY KEY IDENTITY(1,1),
  [RoleName] VARCHAR(50) UNIQUE NOT NULL,
  [CreatedDate] DATETIME2 DEFAULT GETDATE()
);
GO

-- ==========================
-- TABLE: Users
-- ==========================
CREATE TABLE [Users] (
  [UserId] INT PRIMARY KEY IDENTITY(1,1),
  [RoleId] INT,
  [Email] VARCHAR(255) UNIQUE NOT NULL,
  [PasswordHash] VARCHAR(255) NOT NULL,
  [FullName] VARCHAR(200),
  [Phone] VARCHAR(20),
  [Avatar] TEXT,
  [AccountStatus] VARCHAR(20) DEFAULT 'Active',
  [CreatedDate] DATETIME2 DEFAULT GETDATE(),
  [ResetPasswordToken] NVARCHAR(MAX),
  [ResetPasswordTokenExpiry] DATETIME2,
  [OAuthProvider] NVARCHAR(50),
  [OAuthId] NVARCHAR(255),
  [OAuthEmail] NVARCHAR(255)
);
GO

ALTER TABLE [Users] 
ADD FOREIGN KEY ([RoleId]) REFERENCES [UserRoles]([RoleId]);
GO

-- ==========================
-- TABLE: Products
-- ==========================
CREATE TABLE [Products] (
  [ProductId] INT PRIMARY KEY IDENTITY(1,1),
  [SellerId] INT,
  [ProductType] VARCHAR(20) NOT NULL,
  [Title] VARCHAR(255) NOT NULL,
  [Description] TEXT,
  [Price] DECIMAL(18,2) NOT NULL,
  [Brand] VARCHAR(100) NOT NULL,
  [Model] VARCHAR(150),
  [Condition] VARCHAR(50),
  [VehicleType] VARCHAR(50),
  [ManufactureYear] INT,
  [Mileage] INT,
  [BatteryHealth] DECIMAL(5,2),
  [BatteryType] VARCHAR(50),
  [Capacity] DECIMAL(10,2),
  [Voltage] DECIMAL(8,2),
  [CycleCount] INT,
  [Status] VARCHAR(20) DEFAULT 'Draft',
  [VerificationStatus] VARCHAR(20) DEFAULT 'NotRequested',
  [LicensePlate] NVARCHAR(20),
  [CreatedDate] DATETIME2 DEFAULT GETDATE()
);
GO

ALTER TABLE [Products] 
ADD FOREIGN KEY ([SellerId]) REFERENCES [Users]([UserId]);
GO

-- ==========================
-- TABLE: ProductImages
-- ==========================
CREATE TABLE [ProductImages] (
  [ImageId] INT PRIMARY KEY IDENTITY(1,1),
  [ProductId] INT,
  [ImageData] TEXT NOT NULL,
  [CreatedDate] DATETIME2 DEFAULT GETDATE(),
  FOREIGN KEY ([ProductId]) REFERENCES [Products]([ProductId])
);
GO

-- ==========================
-- TABLE: Orders
-- ==========================
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
  [PayoutAmount] DECIMAL(18,2),
  [PayoutStatus] VARCHAR(20) DEFAULT 'Pending',
  [CreatedDate] DATETIME2 DEFAULT GETDATE(),
  [CompletedDate] DATETIME2,
  FOREIGN KEY ([BuyerId]) REFERENCES [Users]([UserId]),
  FOREIGN KEY ([SellerId]) REFERENCES [Users]([UserId]),
  FOREIGN KEY ([ProductId]) REFERENCES [Products]([ProductId])
);
GO

-- ==========================
-- TABLE: Payments
-- ==========================
CREATE TABLE [Payments] (
  [PaymentId] INT PRIMARY KEY IDENTITY(1,1),
  [OrderId] INT,
  [PayerId] INT,
  [ProductId] INT NULL,
  [PaymentType] VARCHAR(20),
  [Amount] DECIMAL(18,2) NOT NULL,
  [PaymentMethod] VARCHAR(50),
  [Status] VARCHAR(20) DEFAULT 'Pending',
  [CreatedDate] DATETIME2 DEFAULT GETDATE(),
  FOREIGN KEY ([OrderId]) REFERENCES [Orders]([OrderId]),
  FOREIGN KEY ([PayerId]) REFERENCES [Users]([UserId]),
  FOREIGN KEY ([ProductId]) REFERENCES [Products]([ProductId])
);
GO
CREATE INDEX IX_Payments_ProductId ON [Payments] (ProductId);
GO

-- ==========================
-- TABLE: FeeSettings
-- ==========================
CREATE TABLE [FeeSettings] (
  [FeeId] INT PRIMARY KEY IDENTITY(1,1),
  [FeeType] VARCHAR(50) NOT NULL,
  [FeeValue] DECIMAL(10,4) NOT NULL,
  [IsActive] BIT DEFAULT 1,
  [CreatedDate] DATETIME2 DEFAULT GETDATE()
);
GO

-- ==========================
-- TABLE: Favorites
-- ==========================
CREATE TABLE [Favorites] (
  [FavoriteId] INT PRIMARY KEY IDENTITY(1,1),
  [UserId] INT,
  [ProductId] INT,
  [CreatedDate] DATETIME2 DEFAULT GETDATE(),
  FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId]),
  FOREIGN KEY ([ProductId]) REFERENCES [Products]([ProductId])
);
GO
CREATE UNIQUE INDEX IX_Favorites_User_Product ON [Favorites]([UserId],[ProductId]);
GO

-- ==========================
-- TABLE: Reviews
-- ==========================
CREATE TABLE [Reviews] (
  [ReviewId] INT PRIMARY KEY IDENTITY(1,1),
  [OrderId] INT,
  [ReviewerId] INT,
  [RevieweeId] INT,
  [Rating] INT,
  [Content] TEXT,
  [CreatedDate] DATETIME2 DEFAULT GETDATE(),
  FOREIGN KEY ([OrderId]) REFERENCES [Orders]([OrderId]),
  FOREIGN KEY ([ReviewerId]) REFERENCES [Users]([UserId]),
  FOREIGN KEY ([RevieweeId]) REFERENCES [Users]([UserId])
);
GO

-- ==========================
-- TABLE: ReportedListings
-- ==========================
CREATE TABLE [ReportedListings] (
  [ReportId] INT PRIMARY KEY IDENTITY(1,1),
  [ProductId] INT,
  [ReporterId] INT,
  [ReportType] VARCHAR(50),
  [ReportReason] VARCHAR(500) NOT NULL,
  [Status] VARCHAR(20) DEFAULT 'Pending',
  [CreatedDate] DATETIME2 DEFAULT GETDATE(),
  FOREIGN KEY ([ProductId]) REFERENCES [Products]([ProductId]),
  FOREIGN KEY ([ReporterId]) REFERENCES [Users]([UserId])
);
GO

-- ==========================
-- TABLE: Notifications
-- ==========================
CREATE TABLE [Notifications] (
  [NotificationId] INT PRIMARY KEY IDENTITY(1,1),
  [UserId] INT,
  [NotificationType] VARCHAR(50),
  [Title] VARCHAR(255) NOT NULL,
  [Content] TEXT,
  [CreatedDate] DATETIME2 DEFAULT GETDATE(),
  FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId])
);
GO

-- ==========================
-- EXTENDED PROPERTIES (giữ lại các phần quan trọng)
-- ==========================
EXEC sp_addextendedproperty 
@name = N'Column_Description', 
@value = 'Active, Locked, Suspended',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table', @level1name = 'Users',
@level2type = N'Column', @level2name = 'AccountStatus';

EXEC sp_addextendedproperty 
@name = N'Column_Description', 
@value = 'OAuth Provider: Google, Facebook, null for regular users',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table', @level1name = 'Users',
@level2type = N'Column', @level2name = 'OAuthProvider';

EXEC sp_addextendedproperty 
@name = N'Column_Description', 
@value = 'Draft, Active, Sold, Expired',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table', @level1name = 'Products',
@level2type = N'Column', @level2name = 'Status';

EXEC sp_addextendedproperty 
@name = N'Column_Description', 
@value = 'Pending, Paid, Failed',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table', @level1name = 'Payments',
@level2type = N'Column', @level2name = 'Status';
GO

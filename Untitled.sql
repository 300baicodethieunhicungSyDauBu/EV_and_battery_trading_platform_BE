CREATE DATABASE Topic2
GO
Use Topic2
go
CREATE TABLE [UserRoles] (
  [RoleId] int PRIMARY KEY IDENTITY(1, 1),
  [RoleName] varchar(50) UNIQUE NOT NULL,
  [CreatedDate] datetime2 DEFAULT (getdate())
)
GO

CREATE TABLE [Users] (
  [UserId] int PRIMARY KEY IDENTITY(1, 1),
  [RoleId] int,
  [Email] varchar(255) UNIQUE NOT NULL,
  [PasswordHash] varchar(255) NOT NULL,
  [FullName] varchar(200),
  [Phone] varchar(20),
  [Avatar] text,
  [AccountStatus] varchar(20) DEFAULT 'Active',
  [CreatedDate] datetime2 DEFAULT (getdate())
)
GO

CREATE TABLE [Products] (
  [ProductId] int PRIMARY KEY IDENTITY(1, 1),
  [SellerId] int,
  [ProductType] varchar(20) NOT NULL,
  [Title] varchar(255) NOT NULL,
  [Description] text,
  [Price] decimal(18,2) NOT NULL,
  [Brand] varchar(100) NOT NULL,
  [Model] varchar(150),
  [Condition] varchar(50),
  [VehicleType] varchar(50),
  [ManufactureYear] int,
  [Mileage] int,
  [BatteryHealth] decimal(5,2),
  [BatteryType] varchar(50),
  [Capacity] decimal(10,2),
  [Voltage] decimal(8,2),
  [CycleCount] int,
  [Status] varchar(20) DEFAULT 'Draft',
  [VerificationStatus] varchar(20) DEFAULT 'NotRequested',
  [CreatedDate] datetime2 DEFAULT (getdate())
)
GO

CREATE TABLE [ProductImages] (
  [ImageId] int PRIMARY KEY IDENTITY(1, 1),
  [ProductId] int,
  [ImageData] text NOT NULL,
  [CreatedDate] datetime2 DEFAULT (getdate())
)
GO

CREATE TABLE [Orders] (
  [OrderId] int PRIMARY KEY IDENTITY(1, 1),
  [BuyerId] int,
  [SellerId] int,
  [ProductId] int,
  [TotalAmount] decimal(18,2) NOT NULL,
  [DepositAmount] decimal(18,2) NOT NULL,
  [Status] varchar(20) DEFAULT 'Pending',
  [DepositStatus] varchar(20) DEFAULT 'Pending',
  [FinalPaymentStatus] varchar(20) DEFAULT 'Pending',
  [PayoutAmount] decimal(18,2),
  [PayoutStatus] varchar(20) DEFAULT 'Pending',
  [CreatedDate] datetime2 DEFAULT (getdate()),
  [CompletedDate] datetime2
)
GO

CREATE TABLE [Payments] (
  [PaymentId] int PRIMARY KEY IDENTITY(1, 1),
  [OrderId] int,
  [PayerId] int,
  [PaymentType] varchar(20),
  [Amount] decimal(18,2) NOT NULL,
  [PaymentMethod] varchar(50),
  [Status] varchar(20) DEFAULT 'Pending',
  [CreatedDate] datetime2 DEFAULT (getdate())
)
GO

CREATE TABLE [FeeSettings] (
  [FeeId] int PRIMARY KEY IDENTITY(1, 1),
  [FeeType] varchar(50) NOT NULL,
  [FeeValue] decimal(10,4) NOT NULL,
  [IsActive] bit DEFAULT (1),
  [CreatedDate] datetime2 DEFAULT (getdate())
)
GO

CREATE TABLE [Favorites] (
  [FavoriteId] int PRIMARY KEY IDENTITY(1, 1),
  [UserId] int,
  [ProductId] int,
  [CreatedDate] datetime2 DEFAULT (getdate())
)
GO

CREATE TABLE [Reviews] (
  [ReviewId] int PRIMARY KEY IDENTITY(1, 1),
  [OrderId] int,
  [ReviewerId] int,
  [RevieweeId] int,
  [Rating] int,
  [Content] text,
  [CreatedDate] datetime2 DEFAULT (getdate())
)
GO

CREATE TABLE [ReportedListings] (
  [ReportId] int PRIMARY KEY IDENTITY(1, 1),
  [ProductId] int,
  [ReporterId] int,
  [ReportType] varchar(50),
  [ReportReason] varchar(500) NOT NULL,
  [Status] varchar(20) DEFAULT 'Pending',
  [CreatedDate] datetime2 DEFAULT (getdate())
)
GO

CREATE TABLE [Notifications] (
  [NotificationId] int PRIMARY KEY IDENTITY(1, 1),
  [UserId] int,
  [NotificationType] varchar(50),
  [Title] varchar(255) NOT NULL,
  [Content] text,
  [CreatedDate] datetime2 DEFAULT (getdate())
)
GO

CREATE UNIQUE INDEX [Favorites_index_0] ON [Favorites] ("UserId", "ProductId")
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Base64 encoded image',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Users',
@level2type = N'Column', @level2name = 'Avatar';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Active, Locked, Suspended',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Users',
@level2type = N'Column', @level2name = 'AccountStatus';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Vehicle, Battery',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Products',
@level2type = N'Column', @level2name = 'ProductType';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Excellent, Good, Fair, Poor',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Products',
@level2type = N'Column', @level2name = 'Condition';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Car, Motorcycle, Bike',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Products',
@level2type = N'Column', @level2name = 'VehicleType';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'km',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Products',
@level2type = N'Column', @level2name = 'Mileage';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '%',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Products',
@level2type = N'Column', @level2name = 'BatteryHealth';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'CarBattery, MotorcycleBattery',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Products',
@level2type = N'Column', @level2name = 'BatteryType';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'kWh',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Products',
@level2type = N'Column', @level2name = 'Capacity';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Draft, Active, Sold, Expired',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Products',
@level2type = N'Column', @level2name = 'Status';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'NotRequested, Requested, Verified',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Products',
@level2type = N'Column', @level2name = 'VerificationStatus';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Base64 encoded image',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ProductImages',
@level2type = N'Column', @level2name = 'ImageData';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Pending, Deposited, Completed, Cancelled',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Orders',
@level2type = N'Column', @level2name = 'Status';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Pending, Paid, Failed',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Orders',
@level2type = N'Column', @level2name = 'DepositStatus';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Pending, Paid, Failed',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Orders',
@level2type = N'Column', @level2name = 'FinalPaymentStatus';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Cash amount for seller after commission',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Orders',
@level2type = N'Column', @level2name = 'PayoutAmount';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Pending, PaidCash',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Orders',
@level2type = N'Column', @level2name = 'PayoutStatus';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Deposit, FinalPayment, Verification',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Payments',
@level2type = N'Column', @level2name = 'PaymentType';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'EWallet, Banking, CreditCard',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Payments',
@level2type = N'Column', @level2name = 'PaymentMethod';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Pending, Success, Failed',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Payments',
@level2type = N'Column', @level2name = 'Status';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'TransactionCommission, ListingFee, VerificationFee',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'FeeSettings',
@level2type = N'Column', @level2name = 'FeeType';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Person giving review',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Reviews',
@level2type = N'Column', @level2name = 'ReviewerId';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Person being reviewed',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Reviews',
@level2type = N'Column', @level2name = 'RevieweeId';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '1-5 stars',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Reviews',
@level2type = N'Column', @level2name = 'Rating';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Fraud, FakeInfo, Scam',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ReportedListings',
@level2type = N'Column', @level2name = 'ReportType';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Pending, Resolved, Dismissed',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ReportedListings',
@level2type = N'Column', @level2name = 'Status';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'OrderUpdate, PaymentSuccess, NewMessage, ProductSold',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Notifications',
@level2type = N'Column', @level2name = 'NotificationType';
GO

ALTER TABLE [Users] ADD FOREIGN KEY ([RoleId]) REFERENCES [UserRoles] ([RoleId])
GO

ALTER TABLE [Products] ADD FOREIGN KEY ([SellerId]) REFERENCES [Users] ([UserId])
GO

ALTER TABLE [ProductImages] ADD FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId])
GO

ALTER TABLE [Orders] ADD FOREIGN KEY ([BuyerId]) REFERENCES [Users] ([UserId])
GO

ALTER TABLE [Orders] ADD FOREIGN KEY ([SellerId]) REFERENCES [Users] ([UserId])
GO

ALTER TABLE [Orders] ADD FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId])
GO

ALTER TABLE [Payments] ADD FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId])
GO

ALTER TABLE [Payments] ADD FOREIGN KEY ([PayerId]) REFERENCES [Users] ([UserId])
GO

ALTER TABLE [Favorites] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
GO

ALTER TABLE [Favorites] ADD FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId])
GO

ALTER TABLE [Reviews] ADD FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([OrderId])
GO

ALTER TABLE [Reviews] ADD FOREIGN KEY ([ReviewerId]) REFERENCES [Users] ([UserId])
GO

ALTER TABLE [Reviews] ADD FOREIGN KEY ([RevieweeId]) REFERENCES [Users] ([UserId])
GO

ALTER TABLE [ReportedListings] ADD FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId])
GO

ALTER TABLE [ReportedListings] ADD FOREIGN KEY ([ReporterId]) REFERENCES [Users] ([UserId])
GO

ALTER TABLE [Notifications] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
GO

-- Khi rollback (Down)  
ALTER TABLE [Products] DROP COLUMN LicensePlate;

ALTER TABLE [Products]
ADD LicensePlate NVARCHAR(20);

select * from Products

-- Thêm field ResetPasswordToken
ALTER TABLE Users 
ADD ResetPasswordToken nvarchar(max) NULL;

-- Thêm field ResetPasswordTokenExpiry  
ALTER TABLE Users 
ADD ResetPasswordTokenExpiry datetime2 NULL;

-- Kiểm tra kết quả
SELECT * FROM Users;
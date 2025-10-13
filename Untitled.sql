Create DATABASE EvAndBatteryTradingPlatform
go
USE EvAndBatteryTradingPlatform
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
  [CreatedDate] datetime2 DEFAULT (getdate()),
  [ResetPasswordToken] nvarchar(max),
  [ResetPasswordTokenExpiry] datetime2
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
  [Transmission] varchar(50),
  [SeatCount] int,
  [LicensePlate] nvarchar(20),
  [BatteryType] varchar(50),
  [BatteryHealth] decimal(5,2),
  [Capacity] decimal(10,2),
  [Voltage] decimal(8,2),
  [BMS] varchar(100),
  [CellType] varchar(50),
  [Status] varchar(20) DEFAULT 'Draft',
  [VerificationStatus] varchar(20) DEFAULT 'NotRequested',
  [RejectionReason] text,
  [CreatedDate] datetime2 DEFAULT (getdate())
)
GO

CREATE TABLE [ProductImages] (
  [ImageId] int PRIMARY KEY IDENTITY(1, 1),
  [ProductId] int,
  [Name] varchar(100),
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
  [FinalPaymentDueDate] datetime2,
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
  [PaymentStatus] varchar(20) DEFAULT 'Pending',
  [CreatedDate] datetime2 DEFAULT (getdate()),
  [TransactionNo] varchar(100),
  [BankCode] varchar(50),
  [BankTranNo] varchar(100),
  [CardType] varchar(50),
  [PayDate] datetime2,
  [ResponseCode] varchar(10),
  [TransactionStatus] varchar(10),
  [SecureHash] varchar(512)
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

CREATE TABLE [Chats] (
  [ChatId] int PRIMARY KEY IDENTITY(1, 1),
  [User1Id] int,
  [User2Id] int,
  [CreatedDate] datetime2 DEFAULT (getdate())
)
GO

CREATE TABLE [Messages] (
  [MessageId] int PRIMARY KEY IDENTITY(1, 1),
  [ChatId] int,
  [SenderId] int,
  [Content] text NOT NULL,
  [IsRead] bit DEFAULT (0),
  [CreatedDate] datetime2 DEFAULT (getdate())
)
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
@value = 'Automatic, Manual',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Products',
@level2type = N'Column', @level2name = 'Transmission';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'CarBattery, MotorcycleBattery, BikeBattery',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Products',
@level2type = N'Column', @level2name = 'BatteryType';
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
@value = 'kWh',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Products',
@level2type = N'Column', @level2name = 'Capacity';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Tên hoặc loại hệ thống quản lý pin (Battery Management System)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Products',
@level2type = N'Column', @level2name = 'BMS';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Loại cell (ví dụ: 18650, 21700, LFP, NMC)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Products',
@level2type = N'Column', @level2name = 'CellType';
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
@value = 'NotRequested, Requested, Verified, Rejected',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Products',
@level2type = N'Column', @level2name = 'VerificationStatus';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Reason when verification is rejected',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Products',
@level2type = N'Column', @level2name = 'RejectionReason';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Type of image: vehicle, registration, etc.',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ProductImages',
@level2type = N'Column', @level2name = 'Name';
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
@value = 'Deadline for buyer to pay remaining balance',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Orders',
@level2type = N'Column', @level2name = 'FinalPaymentDueDate';
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
@value = 'VNPAY, Banking, CreditCard',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Payments',
@level2type = N'Column', @level2name = 'PaymentMethod';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Pending, Success, Failed, Refunded',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Payments',
@level2type = N'Column', @level2name = 'PaymentStatus';
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

ALTER TABLE [Chats] ADD FOREIGN KEY ([User1Id]) REFERENCES [Users] ([UserId])
GO

ALTER TABLE [Chats] ADD FOREIGN KEY ([User2Id]) REFERENCES [Users] ([UserId])
GO

ALTER TABLE [Messages] ADD FOREIGN KEY ([ChatId]) REFERENCES [Chats] ([ChatId])
GO

ALTER TABLE [Messages] ADD FOREIGN KEY ([SenderId]) REFERENCES [Users] ([UserId])
GO

ALTER TABLE Users
ADD OAuthEmail VARCHAR(255) NULL,
    OAuthId VARCHAR(255) NULL,
    OAuthProvider VARCHAR(50) NULL;

ALTER TABLE [Products] 
ADD [CycleCount] INT NULL;
GO

go
INSERT INTO [UserRoles] ([RoleName]) VALUES
('Admin'),
('Member');
go
INSERT INTO [Users] ([RoleId], [Email], [PasswordHash], [FullName], [Phone], [AccountStatus])
VALUES
(1, 'ptdtan43@gmail.com', 'admin123', N'System Administrator', '0902835570', 'Active'),
(2, 'tan@evtrade.com', 'member123', N'Phạm Từ Duy Tân', '0900000002', 'Active'),
(2, 'vinh@evtrade.com', 'member123', N'Nguyễn Vinh', '0900000003', 'Active');
go
INSERT INTO [Products] 
([SellerId], [ProductType], [Title], [Description], [Price], [Brand], [Model], [Condition], [VehicleType], [ManufactureYear], [Mileage], [Transmission], [SeatCount], [LicensePlate], [Status])
VALUES
(2, 'Vehicle', N'VinFast VF e34', N'Used electric car in good condition', 450000000, 'VinFast', 'VF e34', 'Good', 'Car', 2022, 15000, 'Automatic', 5, N'51H-123.45', 'Active'),

SELECT * FROM Products

go

INSERT INTO Products (
    SellerId, ProductType, Title, Description, Price, Brand, Model, Condition, 
    VehicleType, ManufactureYear, Mileage, Transmission, SeatCount, LicensePlate,
    BatteryHealth, BatteryType, Capacity, Voltage, BMS, CellType, CycleCount, Status
)
VALUES (
    2, 'Vehicle', 
    N'VinFast VF e34 2022 - Sedan điện cao cấp', 
    N'Xe VinFast VF e34 đời 2022, đã qua sử dụng, pin còn 85%, chạy 300km, nội thất cao cấp.', 
    450000000, 'VinFast', 'VF e34', 'Good', 
    'Car', 2022, 15000, 'Automatic', 5, N'51H-123.45', 
    85.5, 'CarBattery', 42.0, 400.0, 'VinFast BMS v2.0', 'LFP', 450, 'Active'
);

go

INSERT INTO Products (
    SellerId, ProductType, Title, Description, Price, Brand, Model, Condition,
    BatteryHealth, BatteryType, Capacity, Voltage, BMS, CellType, CycleCount, Status
)
VALUES (
    3, 'Battery',
    N'Pin xe máy điện Yamaha E01 - Dung lượng cao',
    N'Pin Yamaha E01 đã dùng 8 tháng, còn 92% dung lượng, kèm dây sạc và hộp bảo quản.',
    8500000, 'Yamaha', 'E01 Battery Pack', 'Excellent',
    92.0, 'MotorcycleBattery', 2.3, 48.0, 'Yamaha Smart BMS', 'NMC', 180, 'Active'
);


select * from Products where ProductType = 'Vehicle'
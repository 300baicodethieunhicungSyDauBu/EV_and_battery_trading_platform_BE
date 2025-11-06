using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BE.BOs.Models;

public partial class EvandBatteryTradingPlatformContext : DbContext
{
    public EvandBatteryTradingPlatformContext()
    {
    }

    public EvandBatteryTradingPlatformContext(DbContextOptions<EvandBatteryTradingPlatformContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<FeeSetting> FeeSettings { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<ReportedListing> ReportedListings { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(GetConnectionString(), sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(60);
            });
        }
    }

    private string GetConnectionString()
    {
        IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true).Build();
        return configuration["ConnectionStrings:DefaultConnectionString"] ?? string.Empty;
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Favorite>(entity =>
    {
        entity.HasKey(e => e.FavoriteId).HasName("PK__Favorite__CE74FAD5376D3B40");
        entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

        entity.HasOne(d => d.Product).WithMany(p => p.Favorites)
            .HasForeignKey(d => d.ProductId)
            .HasConstraintName("FK__Favorites__Produ__693CA210");

        entity.HasOne(d => d.User).WithMany(p => p.Favorites)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("FK__Favorites__UserI__68487DD7");
    });

    modelBuilder.Entity<FeeSetting>(entity =>
    {
        entity.HasKey(e => e.FeeId).HasName("PK__FeeSetti__B387B22938E8F914");
        entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        entity.Property(e => e.FeeType).HasMaxLength(50);
        entity.Property(e => e.FeeValue).HasColumnType("decimal(10, 4)");
        entity.Property(e => e.IsActive).HasDefaultValue(true);
    });

    modelBuilder.Entity<Notification>(entity =>
    {
        entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E12D3FD203A");
        entity.Property(e => e.Content).HasColumnType("nvarchar(max)");
        entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        entity.Property(e => e.NotificationType).HasMaxLength(50);
        entity.Property(e => e.Title).HasMaxLength(255);

        entity.HasOne(d => d.User).WithMany(p => p.Notifications)
            .HasForeignKey(d => d.UserId)
            .HasConstraintName("FK__Notificat__UserI__6EF57B66");
    });

    modelBuilder.Entity<Order>(entity =>
    {
        entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BCF658D7C8B");
        entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        entity.Property(e => e.DepositAmount).HasColumnType("decimal(18, 2)");
        entity.Property(e => e.DepositStatus).HasMaxLength(20).HasDefaultValue("Pending");
        entity.Property(e => e.FinalPaymentStatus).HasMaxLength(20).HasDefaultValue("Pending");
        entity.Property(e => e.PayoutAmount).HasColumnType("decimal(18, 2)");
        entity.Property(e => e.PayoutStatus).HasMaxLength(20).HasDefaultValue("Pending");
        entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
        entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

        entity.HasOne(d => d.Buyer).WithMany(p => p.OrderBuyers)
            .HasForeignKey(d => d.BuyerId);
        entity.HasOne(d => d.Product).WithMany(p => p.Orders)
            .HasForeignKey(d => d.ProductId);
        entity.HasOne(d => d.Seller).WithMany(p => p.OrderSellers)
            .HasForeignKey(d => d.SellerId);
    });

    modelBuilder.Entity<Payment>(entity =>
    {
        entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A3882005226");
        entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
        entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        entity.Property(e => e.PaymentMethod).HasMaxLength(50);
        entity.Property(e => e.PaymentType).HasMaxLength(20);
        entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
        entity.Property(e => e.TransactionNo).HasMaxLength(100);
        entity.Property(e => e.BankCode).HasMaxLength(50);
        entity.Property(e => e.BankTranNo).HasMaxLength(100);
        entity.Property(e => e.CardType).HasMaxLength(50);
        entity.Property(e => e.ResponseCode).HasMaxLength(10);
        entity.Property(e => e.TransactionStatus).HasMaxLength(10);
        entity.Property(e => e.SecureHash).HasMaxLength(512);

        entity.HasOne(d => d.Order).WithMany(p => p.Payments)
            .HasForeignKey(d => d.OrderId);
        entity.HasOne(d => d.Product).WithMany()
            .HasForeignKey(d => d.ProductId);
        entity.HasOne(d => d.Payer).WithMany(p => p.Payments)
            .HasForeignKey(d => d.PayerId);
    });

    modelBuilder.Entity<Product>(entity =>
    {
        entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6CDF2308A9F");

        entity.Property(e => e.BatteryHealth).HasColumnType("decimal(5, 2)");
        entity.Property(e => e.BatteryType).HasMaxLength(50);
        entity.Property(e => e.Brand).HasMaxLength(100);
        entity.Property(e => e.Capacity).HasColumnType("decimal(10, 2)");
        entity.Property(e => e.Condition).HasMaxLength(50);
        entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
        entity.Property(e => e.Model).HasMaxLength(150);
        entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
        entity.Property(e => e.ProductType).HasMaxLength(20);
        entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Draft");
        entity.Property(e => e.Title).HasMaxLength(255);
        entity.Property(e => e.VehicleType).HasMaxLength(50);
        entity.Property(e => e.LicensePlate).HasMaxLength(20).HasColumnType("nvarchar(20)");
        entity.Property(e => e.VerificationStatus).HasMaxLength(20).HasDefaultValue("NotRequested");
        entity.Property(e => e.RejectionReason).HasMaxLength(500);
        entity.Property(e => e.Voltage).HasColumnType("decimal(8, 2)");
        entity.Property(e => e.Transmission).HasMaxLength(50);
        entity.Property(e => e.BMS).HasMaxLength(100);
        entity.Property(e => e.CellType).HasMaxLength(50);

        entity.HasOne(d => d.Seller).WithMany(p => p.Products)
            .HasForeignKey(d => d.SellerId);
    });

    modelBuilder.Entity<ProductImage>(entity =>
    {
        entity.HasKey(e => e.ImageId).HasName("PK__ProductI__7516F70C16ED296D");
        entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        entity.Property(e => e.ImageData).HasColumnType("nvarchar(max)");
        entity.Property(e => e.Name).HasMaxLength(100);
        entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
            .HasForeignKey(d => d.ProductId);
    });

    modelBuilder.Entity<ReportedListing>(entity =>
    {
        entity.HasKey(e => e.ReportId).HasName("PK__Reported__D5BD48056CFD54E9");
        entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        entity.Property(e => e.ReportReason).HasMaxLength(500);
        entity.Property(e => e.ReportType).HasMaxLength(50);
        entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
        entity.HasOne(d => d.Product).WithMany(p => p.ReportedListings)
            .HasForeignKey(d => d.ProductId);
        entity.HasOne(d => d.Reporter).WithMany(p => p.ReportedListings)
            .HasForeignKey(d => d.ReporterId);
    });

    modelBuilder.Entity<Review>(entity =>
    {
        entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__74BC79CE188E4A76");
        entity.Property(e => e.Content).HasColumnType("nvarchar(max)");
        entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        entity.HasOne(d => d.Order).WithMany(p => p.Reviews)
            .HasForeignKey(d => d.OrderId);
        entity.HasOne(d => d.Reviewee).WithMany(p => p.ReviewReviewees)
            .HasForeignKey(d => d.RevieweeId);
        entity.HasOne(d => d.Reviewer).WithMany(p => p.ReviewReviewers)
            .HasForeignKey(d => d.ReviewerId);
    });

    modelBuilder.Entity<User>(entity =>
    {
        entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C582AC607");
        entity.HasIndex(e => e.Email).IsUnique();

        entity.Property(e => e.AccountStatus).HasMaxLength(20).HasDefaultValue("Active");
        entity.Property(e => e.AccountStatusReason)
            .HasColumnName("AccountStatusReason")
            .HasColumnType("nvarchar(max)");
        entity.Property(e => e.Avatar).HasColumnType("nvarchar(max)");
        entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        entity.Property(e => e.Email).HasMaxLength(255);
        entity.Property(e => e.FullName).HasMaxLength(200);
        entity.Property(e => e.PasswordHash).HasMaxLength(255);
        entity.Property(e => e.Phone).HasMaxLength(20);
        entity.Property(e => e.OAuthProvider).HasMaxLength(50);
        entity.Property(e => e.OAuthId).HasMaxLength(255);
        entity.Property(e => e.OAuthEmail).HasMaxLength(255);
        entity.Property(e => e.ResetPasswordToken).HasColumnType("nvarchar(max)");

        entity.HasOne(d => d.Role).WithMany(p => p.Users)
            .HasForeignKey(d => d.RoleId);
    });

    modelBuilder.Entity<UserRole>(entity =>
    {
        entity.HasKey(e => e.RoleId).HasName("PK__UserRole__8AFACE1AD8916952");
        entity.HasIndex(e => e.RoleName).IsUnique();
        entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        entity.Property(e => e.RoleName).HasMaxLength(50);
    });

    modelBuilder.Entity<Chat>(entity =>
    {
        entity.HasKey(e => e.ChatId).HasName("PK__Chats__A9FBE7C5B3C6F7F2");
        entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        entity.HasOne(d => d.User1).WithMany().HasForeignKey(d => d.User1Id);
        entity.HasOne(d => d.User2).WithMany().HasForeignKey(d => d.User2Id);
    });

    modelBuilder.Entity<Message>(entity =>
    {
        entity.HasKey(e => e.MessageId).HasName("PK__Messages__C87C0C9C4E88ABD4");
        entity.Property(e => e.Content).HasColumnType("nvarchar(max)");
        entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        entity.Property(e => e.IsRead).HasDefaultValue(false);
        entity.HasOne(d => d.Chat).WithMany(p => p.Messages)
            .HasForeignKey(d => d.ChatId);
        entity.HasOne(d => d.Sender).WithMany()
            .HasForeignKey(d => d.SenderId);
    });

    OnModelCreatingPartial(modelBuilder);
}


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

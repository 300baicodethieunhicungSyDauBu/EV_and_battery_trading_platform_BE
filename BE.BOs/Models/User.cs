using System;
using System.Collections.Generic;

namespace BE.BOs.Models;

public partial class User
{
    public int UserId { get; set; }

    public int? RoleId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? Avatar { get; set; }

    public string? AccountStatus { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? ResetPasswordToken { get; set; }

    public DateTime? ResetPasswordTokenExpiry { get; set; }

    // OAuth fields
    public string? OAuthProvider { get; set; } // "Google", "Facebook", null for regular users

    public string? OAuthId { get; set; } // External OAuth ID

    public string? OAuthEmail { get; set; } // Email from OAuth provider

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Order> OrderBuyers { get; set; } = new List<Order>();

    public virtual ICollection<Order> OrderSellers { get; set; } = new List<Order>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<ReportedListing> ReportedListings { get; set; } = new List<ReportedListing>();

    public virtual ICollection<Review> ReviewReviewees { get; set; } = new List<Review>();

    public virtual ICollection<Review> ReviewReviewers { get; set; } = new List<Review>();

    public virtual UserRole? Role { get; set; }
}

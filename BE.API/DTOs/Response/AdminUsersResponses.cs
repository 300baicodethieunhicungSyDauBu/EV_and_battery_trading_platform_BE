using System.Collections.Generic;

namespace BE.API.DTOs.Response
{
    public class AdminUserListItemResponse
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "user"; // user|sub_admin|admin
        public string Status { get; set; } = "active"; // active|suspended|deleted
        public string? AccountStatusReason { get; set; }
        public string Reason { get; set; } = string.Empty; // Deprecated: use AccountStatusReason instead
        public System.DateTime? CreatedAt { get; set; }
        public System.DateTime? LastLoginAt { get; set; }
    }

    public class AdminUserStats
    {
        public int OrderCount { get; set; }
        public int ListingCount { get; set; }
        public int ViolationCount { get; set; }
    }

    public class AdminUserDetailResponse
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Avatar { get; set; }
        public string Role { get; set; } = "user";
        public string Status { get; set; } = "active";
        public string? AccountStatusReason { get; set; }
        public System.DateTime? CreatedAt { get; set; }
        public System.DateTime? LastLoginAt { get; set; }
        public AdminUserStats Stats { get; set; } = new AdminUserStats();
    }

    public class AdminAuditItemResponse
    {
        public string Type { get; set; } = string.Empty; // role_changed|status_changed|reset_password
        public System.DateTime At { get; set; }
        public AuditActor By { get; set; } = new AuditActor();
        public Dictionary<string, string?> Meta { get; set; } = new Dictionary<string, string?>();
    }

    public class AuditActor
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
    }

    public class PageMeta
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public List<string> Sort { get; set; } = new List<string>();
    }

    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public PageMeta Meta { get; set; } = new PageMeta();
    }

    public class AdminUserStatusResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = "active"; // active|suspended|deleted
        public bool CanLogin { get; set; } = true; // true nếu status = active, false nếu suspended hoặc deleted
        public string? AccountStatusReason { get; set; }
        public string Message { get; set; } = string.Empty; // Thông báo về trạng thái
    }
}



namespace BE.API.DTOs.Request
{
    public class AdminUserListQuery
    {
        public string? Search { get; set; }
        public string? Role { get; set; } // user|sub_admin|admin
        public string? Status { get; set; } // active|suspended|deleted
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Sort { get; set; } // e.g., createdAt:desc
    }

    public class AdminUserUpdateRequest
    {
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Avatar { get; set; }
    }

    public class AdminUserRoleRequest
    {
        public string Role { get; set; } = "user"; // user|sub_admin|admin
    }

    public class AdminUserStatusRequest
    {
        public string Status { get; set; } = "active"; // active|suspended|deleted
        public string? Reason { get; set; }
    }
}



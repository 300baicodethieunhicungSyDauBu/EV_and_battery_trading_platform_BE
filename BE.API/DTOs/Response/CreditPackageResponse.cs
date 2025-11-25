namespace BE.API.DTOs.Response
{
    /// <summary>
    /// Response thông tin gói credit cho Admin
    /// </summary>
    public class CreditPackageResponse
    {
        public int FeeId { get; set; }
        
        public string PackageId { get; set; } = null!; // "PostCredit_5"
        
        /// <summary>
        /// Số lượt đăng
        /// </summary>
        public int Credits { get; set; }
        
        /// <summary>
        /// Giá gói (VND)
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Giá mỗi lượt đăng (tự động tính)
        /// </summary>
        public decimal PricePerCredit { get; set; }
        
        /// <summary>
        /// Trạng thái
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Tên gói
        /// </summary>
        public string? PackageName { get; set; }
        
        /// <summary>
        /// Mô tả chi tiết
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Ngày tạo
        /// </summary>
        public DateTime? CreatedDate { get; set; }
        
        /// <summary>
        /// Số lượng đã bán
        /// </summary>
        public int TotalSold { get; set; }
        
        /// <summary>
        /// Tổng doanh thu
        /// </summary>
        public decimal TotalRevenue { get; set; }
    }
    
    /// <summary>
    /// Thông tin người dùng đã mua gói
    /// </summary>
    public class PackagePurchaseHistoryResponse
    {
        public int PaymentId { get; set; }
        
        public int UserId { get; set; }
        
        public string? UserEmail { get; set; }
        
        public string? UserName { get; set; }
        
        public int Credits { get; set; }
        
        public decimal Amount { get; set; }
        
        public string Status { get; set; } = null!; // "Success", "Failed", "Pending"
        
        public DateTime? PurchaseDate { get; set; }
        
        public string? TransactionNo { get; set; }
    }
}

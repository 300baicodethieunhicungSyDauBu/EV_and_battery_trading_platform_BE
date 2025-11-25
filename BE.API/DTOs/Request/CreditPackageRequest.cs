namespace BE.API.DTOs.Request
{
    /// <summary>
    /// Request để tạo/cập nhật gói credit
    /// </summary>
    public class CreditPackageRequest
    {
        /// <summary>
        /// Số lượt đăng (credits) trong gói
        /// </summary>
        public int Credits { get; set; }
        
        /// <summary>
        /// Giá gói (VND)
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Tên gói (VD: "Gói Phổ Biến", "Gói Tiết Kiệm", "Gói Khởi Đầu")
        /// </summary>
        public string? PackageName { get; set; }
        
        /// <summary>
        /// Mô tả chi tiết gói (optional)
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Trạng thái kích hoạt
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

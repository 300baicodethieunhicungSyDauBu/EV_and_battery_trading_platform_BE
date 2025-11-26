namespace BE.API.DTOs.Request
{
    /// <summary>
    /// Request để cập nhật gói credit (CHỈ cho phép sửa 3 fields)
    /// </summary>
    public class UpdateCreditPackageRequest
    {
        /// <summary>
        /// Tên gói (VD: "Gói Phổ Biến", "Gói Hot")
        /// </summary>
        public string? PackageName { get; set; }
        
        /// <summary>
        /// Mô tả chi tiết gói
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Trạng thái hiển thị (true = hiện, false = ẩn)
        /// </summary>
        public bool IsActive { get; set; }
    }
}

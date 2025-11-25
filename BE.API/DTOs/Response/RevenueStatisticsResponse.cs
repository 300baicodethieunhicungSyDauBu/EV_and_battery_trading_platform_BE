namespace BE.API.DTOs.Response
{
    public class RevenueStatisticsResponse
    {
        /// <summary>
        /// Doanh thu từ đơn hàng hoàn thành (Completed orders)
        /// </summary>
        public decimal CompletedOrdersRevenue { get; set; }

        /// <summary>
        /// Doanh thu từ phí kiểm định sản phẩm (Verification fees)
        /// </summary>
        public decimal VerificationRevenue { get; set; }

        /// <summary>
        /// Doanh thu từ đơn hàng bị hủy không hoàn tiền (Cancelled orders with no refund)
        /// </summary>
        public decimal CancelledNoRefundRevenue { get; set; }

        /// <summary>
        /// Doanh thu từ bán gói credit (Credit packages)
        /// </summary>
        public decimal CreditPackagesRevenue { get; set; }

        /// <summary>
        /// Tổng doanh thu (Total revenue)
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Số lượng đơn hàng hoàn thành
        /// </summary>
        public int CompletedOrdersCount { get; set; }

        /// <summary>
        /// Số lượng thanh toán kiểm định
        /// </summary>
        public int VerificationPaymentsCount { get; set; }

        /// <summary>
        /// Số lượng đơn hàng bị hủy không hoàn tiền
        /// </summary>
        public int CancelledNoRefundCount { get; set; }

        /// <summary>
        /// Số lượng gói credit đã bán
        /// </summary>
        public int CreditPackagesSoldCount { get; set; }

        /// <summary>
        /// Chi tiết các đơn hàng bị hủy không hoàn tiền
        /// </summary>
        public List<CancelledNoRefundOrderDetail>? CancelledNoRefundOrders { get; set; }
    }

    public class CancelledNoRefundOrderDetail
    {
        public int OrderId { get; set; }
        public decimal DepositAmount { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string? CancellationReason { get; set; }
        public int? BuyerId { get; set; }
        public string? BuyerName { get; set; }
        public int? SellerId { get; set; }
        public string? SellerName { get; set; }
        public int? ProductId { get; set; }
        public string? ProductTitle { get; set; }
    }
}

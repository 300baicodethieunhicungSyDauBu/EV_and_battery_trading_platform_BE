using BE.API.DTOs.Request;
using BE.API.DTOs.Response;
using BE.BOs.Models;
using BE.REPOs.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE.API.Controllers
{
    /// <summary>
    /// API cho Admin quản lý gói thành viên (Credit Packages)
    /// </summary>
    [ApiController]
    [Route("api/admin/credit-packages")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminCreditPackageController : ControllerBase
    {
        private readonly IFeeSettingsRepo _feeSettingsRepo;
        private readonly IPaymentRepo _paymentRepo;
        private readonly EvandBatteryTradingPlatformContext _context;

        public AdminCreditPackageController(
            IFeeSettingsRepo feeSettingsRepo,
            IPaymentRepo paymentRepo,
            EvandBatteryTradingPlatformContext context)
        {
            _feeSettingsRepo = feeSettingsRepo;
            _paymentRepo = paymentRepo;
            _context = context;
        }

        /// <summary>
        /// ✅ 1. Xem danh sách tất cả các gói credit
        /// GET /api/admin/credit-packages
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<CreditPackageResponse>> GetAllPackages([FromQuery] bool? isActive = null)
        {
            try
            {
                var feeSettings = _feeSettingsRepo.GetAllFeeSettings()
                    .Where(f => f.FeeType != null && f.FeeType.StartsWith("PostCredit_"));

                if (isActive.HasValue)
                {
                    feeSettings = feeSettings.Where(f => f.IsActive == isActive.Value);
                }

                var packages = feeSettings.Select(f =>
                {
                    var creditsStr = f.FeeType!.Replace("PostCredit_", "");
                    if (!int.TryParse(creditsStr, out var credits))
                        return null;

                    var pricePerCredit = credits > 0 ? f.FeeValue / credits : 0;

                    // Thống kê số lượng đã bán
                    var totalSold = _context.Payments
                        .Where(p => p.PaymentType == "PostCredit" 
                                 && p.PostCredits == credits 
                                 && p.Status == "Success")
                        .Count();

                    var totalRevenue = _context.Payments
                        .Where(p => p.PaymentType == "PostCredit" 
                                 && p.PostCredits == credits 
                                 && p.Status == "Success")
                        .Sum(p => (decimal?)p.Amount) ?? 0;

                    return new CreditPackageResponse
                    {
                        FeeId = f.FeeId,
                        PackageId = f.FeeType,
                        Credits = credits,
                        Price = f.FeeValue,
                        PricePerCredit = pricePerCredit,
                        IsActive = f.IsActive ?? false,
                        PackageName = f.PackageName,
                        Description = f.Description,
                        CreatedDate = f.CreatedDate,
                        TotalSold = totalSold,
                        TotalRevenue = totalRevenue
                    };
                })
                .Where(p => p != null)
                .OrderBy(p => p!.Credits)
                .ToList();

                return Ok(packages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ Xem chi tiết 1 gói credit
        /// </summary>
        [HttpGet("{feeId}")]
        public ActionResult<CreditPackageResponse> GetPackageById(int feeId)
        {
            try
            {
                var feeSetting = _feeSettingsRepo.GetFeeSettingById(feeId);
                
                if (feeSetting == null || !feeSetting.FeeType.StartsWith("PostCredit_"))
                {
                    return NotFound("Credit package not found");
                }

                var creditsStr = feeSetting.FeeType.Replace("PostCredit_", "");
                if (!int.TryParse(creditsStr, out var credits))
                {
                    return BadRequest("Invalid package format");
                }

                var pricePerCredit = credits > 0 ? feeSetting.FeeValue / credits : 0;
                var basePrice = 10000m;
                var discountPercent = (int)Math.Round((1 - (pricePerCredit / basePrice)) * 100);

                // Get statistics
                var totalSold = _context.Payments
                    .Where(p => p.PaymentType == "PostCredit" 
                             && p.PostCredits == credits 
                             && p.Status == "Success")
                    .Count();

                var totalRevenue = _context.Payments
                    .Where(p => p.PaymentType == "PostCredit" 
                             && p.PostCredits == credits 
                             && p.Status == "Success")
                    .Sum(p => (decimal?)p.Amount) ?? 0;

                var response = new CreditPackageResponse
                {
                    FeeId = feeSetting.FeeId,
                    PackageId = feeSetting.FeeType,
                    Credits = credits,
                    Price = feeSetting.FeeValue,
                    PricePerCredit = pricePerCredit,
                    IsActive = feeSetting.IsActive ?? false,
                    PackageName = feeSetting.PackageName,
                    Description = feeSetting.Description,
                    CreatedDate = feeSetting.CreatedDate,
                    TotalSold = totalSold,
                    TotalRevenue = totalRevenue
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        /// <summary>
        /// ✅ 2. Cập nhật thông tin gói (CHỈ cho phép sửa: tên, mô tả, trạng thái)
        /// PUT /api/admin/credit-packages/{feeId}
        /// Body: { "packageName": "Gói Hot", "description": "Ưu đãi đặc biệt", "isActive": true }
        /// 
        /// LƯU Ý: KHÔNG cho phép sửa giá và số lượt để đảm bảo công bằng cho người đã mua
        /// </summary>
        [HttpPut("{feeId}")]
        public ActionResult<CreditPackageResponse> UpdatePackage(int feeId, [FromBody] UpdateCreditPackageRequest request)
        {
            try
            {
                var feeSetting = _feeSettingsRepo.GetFeeSettingById(feeId);
                
                if (feeSetting == null || !feeSetting.FeeType.StartsWith("PostCredit_"))
                    return NotFound("Không tìm thấy gói credit");

                // Get current credits from FeeType (không cho phép thay đổi)
                var creditsStr = feeSetting.FeeType.Replace("PostCredit_", "");
                if (!int.TryParse(creditsStr, out var credits))
                    return BadRequest("Định dạng gói không hợp lệ");

                // ✅ CHỈ CHO PHÉP SỬA 3 FIELDS NÀY:
                feeSetting.PackageName = request.PackageName;
                feeSetting.Description = request.Description;
                feeSetting.IsActive = request.IsActive;

                // ❌ KHÔNG CHO PHÉP SỬA:
                // - feeSetting.FeeValue (giá gói)
                // - feeSetting.FeeType (số lượt)
                // Lý do: Đảm bảo công bằng cho người đã mua, tránh sai số liệu

                var updated = _feeSettingsRepo.UpdateFeeSetting(feeSetting);
                var pricePerCredit = credits > 0 ? updated.FeeValue / credits : 0;

                // Get statistics
                var totalSold = _context.Payments
                    .Where(p => p.PaymentType == "PostCredit" 
                             && p.PostCredits == credits 
                             && p.Status == "Success")
                    .Count();

                var totalRevenue = _context.Payments
                    .Where(p => p.PaymentType == "PostCredit" 
                             && p.PostCredits == credits 
                             && p.Status == "Success")
                    .Sum(p => (decimal?)p.Amount) ?? 0;

                var response = new CreditPackageResponse
                {
                    FeeId = updated.FeeId,
                    PackageId = updated.FeeType,
                    Credits = credits,
                    Price = updated.FeeValue,
                    PricePerCredit = pricePerCredit,
                    IsActive = updated.IsActive ?? false,
                    PackageName = updated.PackageName,
                    Description = updated.Description,
                    CreatedDate = updated.CreatedDate,
                    TotalSold = totalSold,
                    TotalRevenue = totalRevenue
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        /// <summary>
        /// ✅ 3. Xem danh sách người dùng đã mua gói cụ thể
        /// GET /api/admin/credit-packages/{feeId}/purchases
        /// </summary>
        [HttpGet("{feeId}/purchases")]
        public ActionResult<IEnumerable<PackagePurchaseHistoryResponse>> GetPackagePurchases(
            int feeId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var feeSetting = _feeSettingsRepo.GetFeeSettingById(feeId);
                
                if (feeSetting == null || !feeSetting.FeeType.StartsWith("PostCredit_"))
                {
                    return NotFound("Credit package not found");
                }

                var creditsStr = feeSetting.FeeType.Replace("PostCredit_", "");
                if (!int.TryParse(creditsStr, out var credits))
                {
                    return BadRequest("Invalid package format");
                }

                // Lấy danh sách payments cho gói này
                var query = _context.Payments
                    .Include(p => p.Payer)
                    .Where(p => p.PaymentType == "PostCredit" && p.PostCredits == credits)
                    .OrderByDescending(p => p.CreatedDate);

                var total = query.Count();
                var purchases = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new PackagePurchaseHistoryResponse
                    {
                        PaymentId = p.PaymentId,
                        UserId = p.PayerId ?? 0,
                        UserEmail = p.Payer != null ? p.Payer.Email : null,
                        UserName = p.Payer != null ? p.Payer.FullName : null,
                        Credits = p.PostCredits ?? 0,
                        Amount = p.Amount,
                        Status = p.Status ?? "Unknown",
                        PurchaseDate = p.PayDate ?? p.CreatedDate,
                        TransactionNo = p.TransactionNo
                    })
                    .ToList();

                return Ok(new
                {
                    PackageInfo = new
                    {
                        FeeId = feeSetting.FeeId,
                        Credits = credits,
                        Price = feeSetting.FeeValue,
                        PackageName = feeSetting.PackageName,
                        Description = feeSetting.Description
                    },
                    TotalRecords = total,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                    Purchases = purchases
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ 4. Thống kê tổng quan về credit packages
        /// GET /api/admin/credit-packages/statistics
        /// </summary>
        [HttpGet("statistics")]
        public ActionResult GetPackageStatistics()
        {
            try
            {
                var allPayments = _context.Payments
                    .Where(p => p.PaymentType == "PostCredit" && p.Status == "Success")
                    .ToList();

                var totalRevenue = allPayments.Sum(p => p.Amount);
                var totalPackagesSold = allPayments.Count;
                var totalCreditsSold = allPayments.Sum(p => p.PostCredits ?? 0);

                var packageBreakdown = allPayments
                    .GroupBy(p => p.PostCredits)
                    .Select(g => new
                    {
                        Credits = g.Key,
                        PackageId = $"PostCredit_{g.Key}",
                        TotalSold = g.Count(),
                        TotalRevenue = g.Sum(p => p.Amount)
                    })
                    .OrderByDescending(x => x.TotalRevenue)
                    .ToList();

                return Ok(new
                {
                    TotalRevenue = totalRevenue,
                    TotalPackagesSold = totalPackagesSold,
                    TotalCreditsSold = totalCreditsSold,
                    AverageOrderValue = totalPackagesSold > 0 ? totalRevenue / totalPackagesSold : 0,
                    PackageBreakdown = packageBreakdown
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

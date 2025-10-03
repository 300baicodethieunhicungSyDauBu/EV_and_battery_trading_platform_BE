using BE.API.DTOs.Request;
using BE.API.DTOs.Response;
using BE.BOs.Models;
using BE.REPOs.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportedListingController : ControllerBase
    {
        private readonly IReportedListingsRepo _reportedListingsRepo;

        public ReportedListingController(IReportedListingsRepo reportedListingsRepo)
        {
            _reportedListingsRepo = reportedListingsRepo;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult<IEnumerable<ReportedListingResponse>> GetAllReportedListings()
        {
            try
            {
                var reportedListings = _reportedListingsRepo.GetAllReportedListings();
                var response = reportedListings.Select(report => new ReportedListingResponse
                {
                    ReportId = report.ReportId,
                    ProductId = report.ProductId ?? 0,
                    ProductTitle = report.Product?.Title ?? "Unknown",
                    ReporterId = report.ReporterId ?? 0,
                    ReporterName = report.Reporter?.FullName ?? "Unknown",
                    ReportType = report.ReportType ?? "",
                    ReportReason = report.ReportReason,
                    Status = report.Status ?? "Pending",
                    CreatedDate = report.CreatedDate
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("{id}")]
        public ActionResult<ReportedListingResponse> GetReportedListingById(int id)
        {
            try
            {
                var reportedListing = _reportedListingsRepo.GetReportedListingById(id);
                if (reportedListing == null)
                {
                    return NotFound("Reported listing not found");
                }

                var response = new ReportedListingResponse
                {
                    ReportId = reportedListing.ReportId,
                    ProductId = reportedListing.ProductId ?? 0,
                    ProductTitle = reportedListing.Product?.Title ?? "Unknown",
                    ReporterId = reportedListing.ReporterId ?? 0,
                    ReporterName = reportedListing.Reporter?.FullName ?? "Unknown",
                    ReportType = reportedListing.ReportType ?? "",
                    ReportReason = reportedListing.ReportReason,
                    Status = reportedListing.Status ?? "Pending",
                    CreatedDate = reportedListing.CreatedDate
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("product/{productId}")]
        public ActionResult<IEnumerable<ReportedListingResponse>> GetReportedListingsByProductId(int productId)
        {
            try
            {
                var reportedListings = _reportedListingsRepo.GetReportedListingsByProductId(productId);
                var response = reportedListings.Select(report => new ReportedListingResponse
                {
                    ReportId = report.ReportId,
                    ProductId = report.ProductId ?? 0,
                    ProductTitle = report.Product?.Title ?? "Unknown",
                    ReporterId = report.ReporterId ?? 0,
                    ReporterName = report.Reporter?.FullName ?? "Unknown",
                    ReportType = report.ReportType ?? "",
                    ReportReason = report.ReportReason,
                    Status = report.Status ?? "Pending",
                    CreatedDate = report.CreatedDate
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("reporter/{reporterId}")]
        public ActionResult<IEnumerable<ReportedListingResponse>> GetReportedListingsByReporterId(int reporterId)
        {
            try
            {
                var reportedListings = _reportedListingsRepo.GetReportedListingsByReporterId(reporterId);
                var response = reportedListings.Select(report => new ReportedListingResponse
                {
                    ReportId = report.ReportId,
                    ProductId = report.ProductId ?? 0,
                    ProductTitle = report.Product?.Title ?? "Unknown",
                    ReporterId = report.ReporterId ?? 0,
                    ReporterName = report.Reporter?.FullName ?? "Unknown",
                    ReportType = report.ReportType ?? "",
                    ReportReason = report.ReportReason,
                    Status = report.Status ?? "Pending",
                    CreatedDate = report.CreatedDate
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("status/{status}")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult<IEnumerable<ReportedListingResponse>> GetReportedListingsByStatus(string status)
        {
            try
            {
                var reportedListings = _reportedListingsRepo.GetReportedListingsByStatus(status);
                var response = reportedListings.Select(report => new ReportedListingResponse
                {
                    ReportId = report.ReportId,
                    ProductId = report.ProductId ?? 0,
                    ProductTitle = report.Product?.Title ?? "Unknown",
                    ReporterId = report.ReporterId ?? 0,
                    ReporterName = report.Reporter?.FullName ?? "Unknown",
                    ReportType = report.ReportType ?? "",
                    ReportReason = report.ReportReason,
                    Status = report.Status ?? "Pending",
                    CreatedDate = report.CreatedDate
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Policy = "MemberOnly")]
        public ActionResult<ReportedListingResponse> CreateReportedListing([FromBody] ReportedListingRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ReportType))
                {
                    return BadRequest("Report type is required");
                }

                if (string.IsNullOrEmpty(request.ReportReason))
                {
                    return BadRequest("Report reason is required");
                }

                var reporterId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (reporterId == 0)
                {
                    return Unauthorized("Invalid user");
                }

                var reportedListing = new ReportedListing
                {
                    ProductId = request.ProductId,
                    ReporterId = reporterId,
                    ReportType = request.ReportType,
                    ReportReason = request.ReportReason
                };

                var createdReport = _reportedListingsRepo.CreateReportedListing(reportedListing);

                var response = new ReportedListingResponse
                {
                    ReportId = createdReport.ReportId,
                    ProductId = createdReport.ProductId ?? 0,
                    ProductTitle = createdReport.Product?.Title ?? "Unknown",
                    ReporterId = createdReport.ReporterId ?? 0,
                    ReporterName = createdReport.Reporter?.FullName ?? "Unknown",
                    ReportType = createdReport.ReportType ?? "",
                    ReportReason = createdReport.ReportReason,
                    Status = createdReport.Status ?? "Pending",
                    CreatedDate = createdReport.CreatedDate
                };

                return CreatedAtAction(nameof(GetReportedListingById), new { id = createdReport.ReportId }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult<ReportedListingResponse> UpdateReportedListingStatus(int id, [FromBody] string status)
        {
            try
            {
                var existingReport = _reportedListingsRepo.GetReportedListingById(id);
                if (existingReport == null)
                {
                    return NotFound("Reported listing not found");
                }

                existingReport.Status = status;
                var updatedReport = _reportedListingsRepo.UpdateReportedListing(existingReport);

                var response = new ReportedListingResponse
                {
                    ReportId = updatedReport.ReportId,
                    ProductId = updatedReport.ProductId ?? 0,
                    ProductTitle = updatedReport.Product?.Title ?? "Unknown",
                    ReporterId = updatedReport.ReporterId ?? 0,
                    ReporterName = updatedReport.Reporter?.FullName ?? "Unknown",
                    ReportType = updatedReport.ReportType ?? "",
                    ReportReason = updatedReport.ReportReason,
                    Status = updatedReport.Status ?? "Pending",
                    CreatedDate = updatedReport.CreatedDate
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public ActionResult DeleteReportedListing(int id)
        {
            try
            {
                var result = _reportedListingsRepo.DeleteReportedListing(id);
                if (!result)
                {
                    return NotFound("Reported listing not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}

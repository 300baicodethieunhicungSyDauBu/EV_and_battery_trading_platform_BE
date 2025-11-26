using BE.API.DTOs.Request;
using BE.API.DTOs.Response;
using BE.BOs.Models;
using BE.REPOs.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewsRepo _reviewsRepo;

        public ReviewController(IReviewsRepo reviewsRepo)
        {
            _reviewsRepo = reviewsRepo;
        }

        // ⭐ XEM TẤT CẢ ĐÁNH GIÁ (Public)
        // Output: Danh sách tất cả reviews
        [HttpGet]
        public ActionResult<IEnumerable<ReviewResponse>> GetAllReviews()
        {
            try
            {
                // Lấy tất cả reviews từ database
                var reviews = _reviewsRepo.GetAllReviews();
                var response = reviews.Select(review => new ReviewResponse
                {
                    ReviewId = review.ReviewId,
                    OrderId = review.OrderId ?? 0,
                    ReviewerId = review.ReviewerId ?? 0,
                    ReviewerName = review.Reviewer?.FullName ?? "Unknown",
                    RevieweeId = review.RevieweeId ?? 0,
                    RevieweeName = review.Reviewee?.FullName ?? "Unknown",
                    Rating = review.Rating ?? 0,
                    Content = review.Content ?? "",
                    CreatedDate = review.CreatedDate
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("{id}")]
        public ActionResult<ReviewResponse> GetReviewById(int id)
        {
            try
            {
                var review = _reviewsRepo.GetReviewById(id);
                if (review == null)
                {
                    return NotFound("Review not found");
                }

                var response = new ReviewResponse
                {
                    ReviewId = review.ReviewId,
                    OrderId = review.OrderId ?? 0,
                    ReviewerId = review.ReviewerId ?? 0,
                    ReviewerName = review.Reviewer?.FullName ?? "Unknown",
                    RevieweeId = review.RevieweeId ?? 0,
                    RevieweeName = review.Reviewee?.FullName ?? "Unknown",
                    Rating = review.Rating ?? 0,
                    Content = review.Content ?? "",
                    CreatedDate = review.CreatedDate
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // XEM ĐÁNH GIÁ CỦA 1 USER (người được đánh giá)
        // Input: revieweeId (userId của người được đánh giá)
        // Output: Danh sách reviews về user đó
        [HttpGet("reviewee/{revieweeId}")]
        public ActionResult<IEnumerable<ReviewResponse>> GetReviewsByRevieweeId(int revieweeId)
        {
            try
            {
                // 1️⃣ Lấy tất cả reviews về user này
                var reviews = _reviewsRepo.GetReviewsByRevieweeId(revieweeId);
                var response = reviews.Select(review => new ReviewResponse
                {
                    ReviewId = review.ReviewId,
                    OrderId = review.OrderId ?? 0,
                    ReviewerId = review.ReviewerId ?? 0,
                    ReviewerName = review.Reviewer?.FullName ?? "Unknown",
                    RevieweeId = review.RevieweeId ?? 0,
                    RevieweeName = review.Reviewee?.FullName ?? "Unknown",
                    Rating = review.Rating ?? 0,
                    Content = review.Content ?? "",
                    CreatedDate = review.CreatedDate
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("reviewer/{reviewerId}")]
        public ActionResult<IEnumerable<ReviewResponse>> GetReviewsByReviewerId(int reviewerId)
        {
            try
            {
                var reviews = _reviewsRepo.GetReviewsByReviewerId(reviewerId);
                var response = reviews.Select(review => new ReviewResponse
                {
                    ReviewId = review.ReviewId,
                    OrderId = review.OrderId ?? 0,
                    ReviewerId = review.ReviewerId ?? 0,
                    ReviewerName = review.Reviewer?.FullName ?? "Unknown",
                    RevieweeId = review.RevieweeId ?? 0,
                    RevieweeName = review.Reviewee?.FullName ?? "Unknown",
                    Rating = review.Rating ?? 0,
                    Content = review.Content ?? "",
                    CreatedDate = review.CreatedDate
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("order/{orderId}")]
        public ActionResult<IEnumerable<ReviewResponse>> GetReviewsByOrderId(int orderId)
        {
            try
            {
                var reviews = _reviewsRepo.GetReviewsByOrderId(orderId);
                var response = reviews.Select(review => new ReviewResponse
                {
                    ReviewId = review.ReviewId,
                    OrderId = review.OrderId ?? 0,
                    ReviewerId = review.ReviewerId ?? 0,
                    ReviewerName = review.Reviewer?.FullName ?? "Unknown",
                    RevieweeId = review.RevieweeId ?? 0,
                    RevieweeName = review.Reviewee?.FullName ?? "Unknown",
                    Rating = review.Rating ?? 0,
                    Content = review.Content ?? "",
                    CreatedDate = review.CreatedDate
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
        public ActionResult<ReviewResponse> CreateReview([FromBody] ReviewRequest request)
        {
            try
            {
                if (request.Rating < 1 || request.Rating > 5)
                {
                    return BadRequest("Rating must be between 1 and 5");
                }

                if (string.IsNullOrEmpty(request.Content))
                {
                    return BadRequest("Content is required");
                }

                var reviewerId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (reviewerId == 0)
                {
                    return Unauthorized("Invalid user");
                }

                // Get the order to determine the seller (reviewee)
                using var context = new EvandBatteryTradingPlatformContext();
                var order = context.Orders
                    .Include(o => o.Seller)
                    .Include(o => o.Buyer)
                    .FirstOrDefault(o => o.OrderId == request.OrderId);

                if (order == null)
                {
                    return NotFound("Order not found");
                }

                // Validate that the buyer is the one creating the review
                if (order.BuyerId != reviewerId)
                {
                    return Forbid("You can only review sellers for orders you have placed");
                }

                // Validate that the reviewer is not reviewing themselves
                if (order.SellerId == reviewerId)
                {
                    return BadRequest("You cannot review yourself");
                }

                // Automatically set the RevieweeId from the order's SellerId
                var revieweeId = order.SellerId ?? 0;
                if (revieweeId == 0)
                {
                    return BadRequest("Order does not have a valid seller");
                }

                var review = new Review
                {
                    OrderId = request.OrderId,
                    ReviewerId = reviewerId,
                    RevieweeId = revieweeId, // Use seller from order, not from request
                    Rating = request.Rating,
                    Content = request.Content
                };

                var createdReview = _reviewsRepo.CreateReview(review);

                var response = new ReviewResponse
                {
                    ReviewId = createdReview.ReviewId,
                    OrderId = createdReview.OrderId ?? 0,
                    ReviewerId = createdReview.ReviewerId ?? 0,
                    ReviewerName = createdReview.Reviewer?.FullName ?? "Unknown",
                    RevieweeId = createdReview.RevieweeId ?? 0,
                    RevieweeName = createdReview.Reviewee?.FullName ?? "Unknown",
                    Rating = createdReview.Rating ?? 0,
                    Content = createdReview.Content ?? "",
                    CreatedDate = createdReview.CreatedDate
                };

                return CreatedAtAction(nameof(GetReviewById), new { id = createdReview.ReviewId }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "MemberOnly")]
        public ActionResult<ReviewResponse> UpdateReview(int id, [FromBody] ReviewRequest request)
        {
            try
            {
                if (request.Rating < 1 || request.Rating > 5)
                {
                    return BadRequest("Rating must be between 1 and 5");
                }

                if (string.IsNullOrEmpty(request.Content))
                {
                    return BadRequest("Content is required");
                }

                var existingReview = _reviewsRepo.GetReviewById(id);
                if (existingReview == null)
                {
                    return NotFound("Review not found");
                }

                var reviewerId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (existingReview.ReviewerId != reviewerId)
                {
                    return Forbid("You can only update your own reviews");
                }

                existingReview.Rating = request.Rating;
                existingReview.Content = request.Content;

                var updatedReview = _reviewsRepo.UpdateReview(existingReview);

                var response = new ReviewResponse
                {
                    ReviewId = updatedReview.ReviewId,
                    OrderId = updatedReview.OrderId ?? 0,
                    ReviewerId = updatedReview.ReviewerId ?? 0,
                    ReviewerName = updatedReview.Reviewer?.FullName ?? "Unknown",
                    RevieweeId = updatedReview.RevieweeId ?? 0,
                    RevieweeName = updatedReview.Reviewee?.FullName ?? "Unknown",
                    Rating = updatedReview.Rating ?? 0,
                    Content = updatedReview.Content ?? "",
                    CreatedDate = updatedReview.CreatedDate
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "MemberOnly")]
        public ActionResult DeleteReview(int id)
        {
            try
            {
                var existingReview = _reviewsRepo.GetReviewById(id);
                if (existingReview == null)
                {
                    return NotFound("Review not found");
                }

                var reviewerId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                if (existingReview.ReviewerId != reviewerId)
                {
                    return Forbid("You can only delete your own reviews");
                }

                var result = _reviewsRepo.DeleteReview(id);
                if (!result)
                {
                    return NotFound("Review not found");
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

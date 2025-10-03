using BE.BOs.Models;

namespace BE.REPOs.Interface
{
    public interface IReviewsRepo
    {
        List<Review> GetAllReviews();
        Review? GetReviewById(int reviewId);
        List<Review> GetReviewsByRevieweeId(int revieweeId);
        List<Review> GetReviewsByReviewerId(int reviewerId);
        List<Review> GetReviewsByOrderId(int orderId);
        Review CreateReview(Review review);
        Review UpdateReview(Review review);
        bool DeleteReview(int reviewId);
    }
}

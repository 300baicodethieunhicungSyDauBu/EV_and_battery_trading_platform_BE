using BE.BOs.Models;
using BE.DAOs;
using BE.REPOs.Interface;

namespace BE.REPOs.Implementation
{
    public class ReviewsRepo : IReviewsRepo
    {
        public List<Review> GetAllReviews()
        {
            return ReviewsDAO.Instance.GetAllReviews();
        }

        public Review? GetReviewById(int reviewId)
        {
            return ReviewsDAO.Instance.GetReviewById(reviewId);
        }

        public List<Review> GetReviewsByRevieweeId(int revieweeId)
        {
            return ReviewsDAO.Instance.GetReviewsByRevieweeId(revieweeId);
        }

        public List<Review> GetReviewsByReviewerId(int reviewerId)
        {
            return ReviewsDAO.Instance.GetReviewsByReviewerId(reviewerId);
        }

        public List<Review> GetReviewsByOrderId(int orderId)
        {
            return ReviewsDAO.Instance.GetReviewsByOrderId(orderId);
        }

        public Review CreateReview(Review review)
        {
            return ReviewsDAO.Instance.CreateReview(review);
        }

        public Review UpdateReview(Review review)
        {
            return ReviewsDAO.Instance.UpdateReview(review);
        }

        public bool DeleteReview(int reviewId)
        {
            return ReviewsDAO.Instance.DeleteReview(reviewId);
        }
    }
}

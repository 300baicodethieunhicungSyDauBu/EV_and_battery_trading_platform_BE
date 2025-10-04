using BE.BOs.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE.DAOs
{
    public class ReviewsDAO
    {
        private static ReviewsDAO? instance;
        private static readonly object lockObject = new object();

        public static ReviewsDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new ReviewsDAO();
                        }
                    }
                }
                return instance;
            }
        }

        private ReviewsDAO() { }

        public List<Review> GetAllReviews()
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .Include(r => r.Order)
                .ToList();
        }

        public Review? GetReviewById(int reviewId)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .Include(r => r.Order)
                .FirstOrDefault(r => r.ReviewId == reviewId);
        }

        public List<Review> GetReviewsByRevieweeId(int revieweeId)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .Include(r => r.Order)
                .Where(r => r.RevieweeId == revieweeId)
                .ToList();
        }

        public List<Review> GetReviewsByReviewerId(int reviewerId)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .Include(r => r.Order)
                .Where(r => r.ReviewerId == reviewerId)
                .ToList();
        }

        public List<Review> GetReviewsByOrderId(int orderId)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            return context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .Include(r => r.Order)
                .Where(r => r.OrderId == orderId)
                .ToList();
        }

        public Review CreateReview(Review review)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            review.CreatedDate = DateTime.Now;
            context.Reviews.Add(review);
            context.SaveChanges();
            return review;
        }

        public Review UpdateReview(Review review)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            var existingReview = context.Reviews.FirstOrDefault(r => r.ReviewId == review.ReviewId);
            if (existingReview != null)
            {
                existingReview.Rating = review.Rating;
                existingReview.Content = review.Content;
                context.SaveChanges();
                return existingReview;
            }
            return review;
        }

        public bool DeleteReview(int reviewId)
        {
            using var context = new EvandBatteryTradingPlatformContext();
            var review = context.Reviews.FirstOrDefault(r => r.ReviewId == reviewId);
            if (review != null)
            {
                context.Reviews.Remove(review);
                context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}

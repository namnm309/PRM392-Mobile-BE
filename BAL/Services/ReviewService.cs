using BAL.DTOs.Review;
using DAL.Models;
using DAL.Repositories;

namespace BAL.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IReviewReplyRepository _reviewReplyRepository;
        private readonly IOrderItemRepository _orderItemRepository;

        public ReviewService(
            IReviewRepository reviewRepository,
            IReviewReplyRepository reviewReplyRepository,
            IOrderItemRepository orderItemRepository)
        {
            _reviewRepository = reviewRepository;
            _reviewReplyRepository = reviewReplyRepository;
            _orderItemRepository = orderItemRepository;
        }

        public async Task<IEnumerable<ReviewResponseDto>> GetReviewsByProductIdAsync(Guid productId)
        {
            var reviews = await _reviewRepository.GetByProductIdAsync(productId);
            return reviews.Select(MapToDto);
        }

        public async Task<ReviewResponseDto> CreateReviewAsync(Guid userId, CreateReviewRequestDto request)
        {
            var hasPurchased = await _orderItemRepository.HasUserPurchasedProductAsync(userId, request.ProductId);
            if (!hasPurchased)
            {
                throw new InvalidOperationException("You can only review products you have successfully received");
            }

            var existingReview = await _reviewRepository.GetByUserIdAndProductIdAsync(userId, request.ProductId);
            if (existingReview != null)
            {
                throw new InvalidOperationException("You have already reviewed this product");
            }

            if (request.Rating < 1 || request.Rating > 5)
            {
                throw new ArgumentException("Rating must be between 1 and 5");
            }

            var review = new Review
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProductId = request.ProductId,
                Rating = request.Rating,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdReview = await _reviewRepository.AddAsync(review);
            var reviewWithDetails = await _reviewRepository.GetByIdWithReplyAsync(createdReview.Id);
            return MapToDto(reviewWithDetails!);
        }

        public async Task<ReviewReplyResponseDto> ReplyToReviewAsync(Guid reviewId, Guid staffId, CreateReviewReplyRequestDto request)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
            {
                throw new InvalidOperationException("Review not found");
            }

            var existingReply = await _reviewReplyRepository.GetByReviewIdAsync(reviewId);
            if (existingReply != null)
            {
                throw new InvalidOperationException("This review has already been replied to");
            }

            var reply = new ReviewReply
            {
                Id = Guid.NewGuid(),
                ReviewId = reviewId,
                StaffId = staffId,
                ReplyContent = request.ReplyContent,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _reviewReplyRepository.AddAsync(reply);
            var replyWithDetails = await _reviewReplyRepository.GetByReviewIdAsync(reviewId);
            return MapReplyToDto(replyWithDetails!);
        }

        public async Task<bool> DeleteReviewAsync(Guid reviewId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
                return false;

            var existingReply = await _reviewReplyRepository.GetByReviewIdAsync(reviewId);
            if (existingReply != null)
            {
                await _reviewReplyRepository.DeleteAsync(existingReply.Id);
            }

            return await _reviewRepository.DeleteAsync(reviewId);
        }

        private static ReviewResponseDto MapToDto(Review review)
        {
            return new ReviewResponseDto
            {
                Id = review.Id,
                UserId = review.UserId,
                UserName = review.User?.FullName ?? "Unknown",
                UserAvatarUrl = review.User?.AvatarUrl,
                ProductId = review.ProductId,
                Rating = review.Rating,
                Content = review.Content,
                Reply = review.Reply != null ? MapReplyToDto(review.Reply) : null,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt
            };
        }

        private static ReviewReplyResponseDto MapReplyToDto(ReviewReply reply)
        {
            return new ReviewReplyResponseDto
            {
                Id = reply.Id,
                ReviewId = reply.ReviewId,
                StaffId = reply.StaffId,
                StaffName = reply.Staff?.FullName ?? "Staff",
                ReplyContent = reply.ReplyContent,
                CreatedAt = reply.CreatedAt
            };
        }
    }
}

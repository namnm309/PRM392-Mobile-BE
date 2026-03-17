using BAL.DTOs.Review;

namespace BAL.Services
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewResponseDto>> GetReviewsByProductIdAsync(Guid productId);
        Task<ReviewResponseDto> CreateReviewAsync(Guid userId, CreateReviewRequestDto request);
        Task<ReviewReplyResponseDto> ReplyToReviewAsync(Guid reviewId, Guid staffId, CreateReviewReplyRequestDto request);
        Task<bool> DeleteReviewAsync(Guid reviewId);
    }
}

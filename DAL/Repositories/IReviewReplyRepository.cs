using DAL.Models;

namespace DAL.Repositories
{
    public interface IReviewReplyRepository : IRepository<ReviewReply>
    {
        Task<ReviewReply?> GetByReviewIdAsync(Guid reviewId);
        Task<bool> HasReplyAsync(Guid reviewId);
    }
}

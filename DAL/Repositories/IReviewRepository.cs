using DAL.Models;

namespace DAL.Repositories
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<IEnumerable<Review>> GetByProductIdAsync(Guid productId);
        Task<Review?> GetByIdWithReplyAsync(Guid id);
        Task<Review?> GetByUserIdAndProductIdAsync(Guid userId, Guid productId);
        Task<bool> HasUserReviewedAsync(Guid userId, Guid productId);
    }
}

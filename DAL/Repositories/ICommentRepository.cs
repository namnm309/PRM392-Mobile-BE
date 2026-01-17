using DAL.Models;

namespace DAL.Repositories
{
    /// <summary>
    /// Comment repository interface
    /// </summary>
    public interface ICommentRepository : IRepository<Comment>
    {
        Task<IEnumerable<Comment>> GetByProductIdAsync(Guid productId);
        Task<Comment?> GetByIdWithReplyAsync(Guid id);
        Task<Comment?> GetByUserIdAndProductIdAsync(Guid userId, Guid productId);
        Task<bool> HasUserCommentedAsync(Guid userId, Guid productId);
    }
}

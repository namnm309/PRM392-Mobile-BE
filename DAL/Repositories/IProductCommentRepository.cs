using DAL.Models;

namespace DAL.Repositories
{
    public interface IProductCommentRepository : IRepository<ProductComment>
    {
        Task<IEnumerable<ProductComment>> GetByProductIdAsync(Guid productId);
        Task<ProductComment?> GetByIdWithRepliesAsync(Guid id);
    }
}

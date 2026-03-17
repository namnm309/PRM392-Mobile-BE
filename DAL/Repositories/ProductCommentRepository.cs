using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class ProductCommentRepository : Repository<ProductComment>, IProductCommentRepository
    {
        public ProductCommentRepository(TechStoreContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ProductComment>> GetByProductIdAsync(Guid productId)
        {
            var allComments = await _dbSet
                .Include(c => c.User)
                .Where(c => c.ProductId == productId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return allComments;
        }

        public async Task<ProductComment?> GetByIdWithRepliesAsync(Guid id)
        {
            return await _dbSet
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}

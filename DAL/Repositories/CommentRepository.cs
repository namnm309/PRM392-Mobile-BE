using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    /// <summary>
    /// Comment repository implementation
    /// </summary>
    public class CommentRepository : Repository<Comment>, ICommentRepository
    {
        public CommentRepository(TechStoreContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Comment>> GetByProductIdAsync(Guid productId)
        {
            return await _dbSet
                .Include(c => c.User)
                .Include(c => c.Reply)
                    .ThenInclude(r => r != null ? r.Staff : null)
                .Where(c => c.ProductId == productId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment?> GetByIdWithReplyAsync(Guid id)
        {
            return await _dbSet
                .Include(c => c.User)
                .Include(c => c.Reply)
                    .ThenInclude(r => r != null ? r.Staff : null)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Comment?> GetByUserIdAndProductIdAsync(Guid userId, Guid productId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
        }

        public async Task<bool> HasUserCommentedAsync(Guid userId, Guid productId)
        {
            return await _dbSet
                .AnyAsync(c => c.UserId == userId && c.ProductId == productId);
        }
    }
}

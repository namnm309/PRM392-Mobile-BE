using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        public ReviewRepository(TechStoreContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Review>> GetByProductIdAsync(Guid productId)
        {
            return await _dbSet
                .Include(r => r.User)
                .Include(r => r.Reply)
                    .ThenInclude(rr => rr != null ? rr.Staff : null)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review?> GetByIdWithReplyAsync(Guid id)
        {
            return await _dbSet
                .Include(r => r.User)
                .Include(r => r.Reply)
                    .ThenInclude(rr => rr != null ? rr.Staff : null)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Review?> GetByUserIdAndProductIdAsync(Guid userId, Guid productId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId);
        }

        public async Task<bool> HasUserReviewedAsync(Guid userId, Guid productId)
        {
            return await _dbSet
                .AnyAsync(r => r.UserId == userId && r.ProductId == productId);
        }
    }
}

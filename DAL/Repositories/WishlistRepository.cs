using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class WishlistRepository : Repository<WishlistItem>, IWishlistRepository
    {
        public WishlistRepository(TechStoreContext context) : base(context) { }

        public async Task<IEnumerable<WishlistItem>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(w => w.Product)
                    .ThenInclude(p => p.ProductImages)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }

        public async Task<WishlistItem?> GetByUserIdAndProductIdAsync(Guid userId, Guid productId)
        {
            return await _dbSet
                .Include(w => w.Product)
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
        }

        public async Task<bool> IsProductInWishlistAsync(Guid userId, Guid productId)
        {
            return await _dbSet.AnyAsync(w => w.UserId == userId && w.ProductId == productId);
        }

        public async Task<int> GetWishlistCountAsync(Guid userId)
        {
            return await _dbSet.CountAsync(w => w.UserId == userId);
        }
    }
}

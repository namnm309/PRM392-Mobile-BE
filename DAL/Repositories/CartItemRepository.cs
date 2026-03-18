using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    /// <summary>
    /// CartItem repository implementation
    /// </summary>
    public class CartItemRepository : Repository<CartItem>, ICartItemRepository
    {
        public CartItemRepository(TechStoreContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CartItem>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(c => c.Product)
                .Include(c => c.Variant)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<CartItem?> GetByUserIdAndProductIdAsync(Guid userId, Guid productId, Guid? variantId)
        {
            return await _dbSet
                .Include(c => c.Product)
                .Include(c => c.Variant)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId && c.VariantId == variantId);
        }

        public async Task<CartItem?> GetByIdWithProductAsync(Guid id)
        {
            return await _dbSet
                .Include(c => c.Product)
                .Include(c => c.Variant)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> ClearCartByUserIdAsync(Guid userId)
        {
            var items = await _dbSet.Where(c => c.UserId == userId).ToListAsync();
            if (!items.Any())
                return false;

            _dbSet.RemoveRange(items);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCartItemsByIdsAsync(Guid userId, List<Guid> cartItemIds)
        {
            if (cartItemIds == null || !cartItemIds.Any())
                return false;

            // Only delete items that belong to the user
            var items = await _dbSet
                .Where(c => c.UserId == userId && cartItemIds.Contains(c.Id))
                .ToListAsync();

            if (!items.Any())
                return false;

            _dbSet.RemoveRange(items);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCartItemCountAsync(Guid userId)
        {
            return await _dbSet.Where(c => c.UserId == userId).CountAsync();
        }
    }
}

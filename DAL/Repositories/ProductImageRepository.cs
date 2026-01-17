using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    /// <summary>
    /// ProductImage repository implementation
    /// </summary>
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        public ProductImageRepository(TechStoreContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ProductImage>> GetByProductIdAsync(Guid productId)
        {
            return await _dbSet
                .Where(pi => pi.ProductId == productId)
                .OrderBy(pi => pi.DisplayOrder)
                .ThenBy(pi => pi.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductImage>> GetByProductIdAndTypeAsync(Guid productId, string imageType)
        {
            return await _dbSet
                .Where(pi => pi.ProductId == productId && pi.ImageType == imageType)
                .OrderBy(pi => pi.DisplayOrder)
                .ToListAsync();
        }

        public async Task<bool> DeleteByProductIdAsync(Guid productId)
        {
            var images = await _dbSet.Where(pi => pi.ProductId == productId).ToListAsync();
            if (!images.Any())
                return false;

            _dbSet.RemoveRange(images);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ProductImage?> GetMainImageByProductIdAsync(Guid productId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(pi => pi.ProductId == productId && pi.ImageType == "Main");
        }
    }
}

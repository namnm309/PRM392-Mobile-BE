using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class ProductVariantRepository : Repository<ProductVariant>, IProductVariantRepository
    {
        public ProductVariantRepository(TechStoreContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ProductVariant>> GetByProductIdAsync(Guid productId, bool? isActive = null)
        {
            var query = _dbSet.Where(v => v.ProductId == productId);
            if (isActive.HasValue)
                query = query.Where(v => v.IsActive == isActive.Value);

            return await query
                .OrderBy(v => v.DisplayOrder)
                .ThenBy(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<ProductVariant?> GetByIdAndProductIdAsync(Guid id, Guid productId)
        {
            return await _dbSet.FirstOrDefaultAsync(v => v.Id == id && v.ProductId == productId);
        }
    }
}


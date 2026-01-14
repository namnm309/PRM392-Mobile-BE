using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    /// <summary>
    /// Product repository implementation
    /// </summary>
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(TechStoreContext context) : base(context)
        {
        }

        public async Task<Product?> GetByIdWithTrackingAsync(Guid id)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages.OrderBy(pi => pi.DisplayOrder))
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _dbSet.Where(p => p.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAllWithFiltersAsync(bool? isActive = null, Guid? categoryId = null, Guid? brandId = null)
        {
            var query = _dbSet.AsQueryable();

            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId.Value);

            return await query
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages.OrderBy(pi => pi.DisplayOrder))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchByNameAsync(string name, bool? isActive = null)
        {
            var query = _dbSet.Where(p => p.Name.Contains(name));

            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            return await query
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages.OrderBy(pi => pi.DisplayOrder))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchByBrandIdAsync(Guid brandId, bool? isActive = null)
        {
            var query = _dbSet.Where(p => p.BrandId == brandId);

            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            return await query
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages.OrderBy(pi => pi.DisplayOrder))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetSimilarProductsAsync(Guid productId, int limit = 5)
        {
            var product = await GetByIdWithDetailsAsync(productId);
            if (product == null)
                return new List<Product>();

            var query = _dbSet.Where(p => p.Id != productId && p.IsActive);

            // Find products with same category or brand
            if (product.CategoryId.HasValue || product.BrandId.HasValue)
            {
                var categoryId = product.CategoryId;
                var brandId = product.BrandId;

                query = query.Where(p => 
                    (categoryId.HasValue && p.CategoryId == categoryId.Value) ||
                    (brandId.HasValue && p.BrandId == brandId.Value)
                );
            }
            else
            {
                // If product has no category or brand, return empty
                return new List<Product>();
            }

            return await query
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages.OrderBy(pi => pi.DisplayOrder))
                .Take(limit)
                .ToListAsync();
        }

        public async Task<bool> IsProductAvailableAsync(Guid productId, int quantity)
        {
            var product = await _dbSet.FirstOrDefaultAsync(p => p.Id == productId);
            return product != null && product.IsActive && product.Stock >= quantity;
        }
    }
}

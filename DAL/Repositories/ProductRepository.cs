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

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _dbSet.Where(p => p.IsActive).ToListAsync();
        }

        public async Task<bool> IsProductAvailableAsync(Guid productId, int quantity)
        {
            var product = await _dbSet.FirstOrDefaultAsync(p => p.Id == productId);
            return product != null && product.IsActive && product.Stock >= quantity;
        }
    }
}

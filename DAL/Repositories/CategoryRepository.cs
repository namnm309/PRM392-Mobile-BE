using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    /// <summary>
    /// Category repository implementation
    /// </summary>
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(TechStoreContext context) : base(context)
        {
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task<IEnumerable<Category>> GetCategoriesWithChildrenAsync()
        {
            var query = _dbSet
                .Include(c => c.CategoryBrands)
                    .ThenInclude(cb => cb.Brand);

            return await query
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(c => c.CategoryBrands)
                    .ThenInclude(cb => cb.Brand)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}

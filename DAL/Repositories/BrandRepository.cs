using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    /// <summary>
    /// Brand repository implementation
    /// </summary>
    public class BrandRepository : Repository<Brand>, IBrandRepository
    {
        public BrandRepository(TechStoreContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Brand>> GetActiveBrandsAsync()
        {
            return await _dbSet.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();
        }

        public async Task<Brand?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(b => b.Name == name);
        }
    }
}

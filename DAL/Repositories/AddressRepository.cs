using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    /// <summary>
    /// Address repository implementation
    /// </summary>
    public class AddressRepository : Repository<Address>, IAddressRepository
    {
        public AddressRepository(TechStoreContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Address>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet.Where(a => a.UserId == userId).OrderByDescending(a => a.IsPrimary).ThenByDescending(a => a.CreatedAt).ToListAsync();
        }

        public async Task<Address?> GetPrimaryAddressByUserIdAsync(Guid userId)
        {
            return await _dbSet.FirstOrDefaultAsync(a => a.UserId == userId && a.IsPrimary);
        }

        public async Task<Address?> GetByIdAndUserIdAsync(Guid id, Guid userId)
        {
            return await _dbSet.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        }

        public async Task<bool> HasPrimaryAddressAsync(Guid userId)
        {
            return await _dbSet.AnyAsync(a => a.UserId == userId && a.IsPrimary);
        }
    }
}

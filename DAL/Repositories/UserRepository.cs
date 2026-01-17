using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    /// <summary>
    /// User repository implementation
    /// </summary>
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(TechStoreContext context) : base(context)
        {
        }

        public async Task<User?> GetByClerkIdAsync(string clerkId)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.ClerkId == clerkId);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task<bool> IsEmailExistsAsync(string email, Guid? excludeUserId = null)
        {
            var query = _dbSet.Where(u => u.Email == email);
            
            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.Id != excludeUserId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> IsClerkIdExistsAsync(string clerkId, Guid? excludeUserId = null)
        {
            var query = _dbSet.Where(u => u.ClerkId == clerkId);
            
            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.Id != excludeUserId.Value);
            }

            return await query.AnyAsync();
        }
    }
}

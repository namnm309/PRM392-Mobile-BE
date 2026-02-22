using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class LinkedAccountRepository : Repository<LinkedAccount>, ILinkedAccountRepository
    {
        public LinkedAccountRepository(TechStoreContext context) : base(context) { }

        public async Task<IEnumerable<LinkedAccount>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(la => la.UserId == userId)
                .OrderByDescending(la => la.LinkedAt)
                .ToListAsync();
        }

        public async Task<LinkedAccount?> GetByUserIdAndProviderAsync(Guid userId, string provider)
        {
            return await _dbSet.FirstOrDefaultAsync(la => la.UserId == userId && la.Provider == provider);
        }

        public async Task<LinkedAccount?> GetByProviderUserIdAsync(string provider, string providerUserId)
        {
            return await _dbSet
                .Include(la => la.User)
                .FirstOrDefaultAsync(la => la.Provider == provider && la.ProviderUserId == providerUserId);
        }
    }
}

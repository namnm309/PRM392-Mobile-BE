using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class MembershipTierRepository : Repository<MembershipTier>, IMembershipTierRepository
    {
        public MembershipTierRepository(TechStoreContext context) : base(context) { }

        public async Task<IEnumerable<MembershipTier>> GetAllActiveAsync()
        {
            return await _dbSet
                .Where(t => t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .ToListAsync();
        }

        public async Task<MembershipTier?> GetTierByPointsAsync(int points)
        {
            return await _dbSet
                .Where(t => t.IsActive && points >= t.MinPoints && points <= t.MaxPoints)
                .OrderByDescending(t => t.MinPoints)
                .FirstOrDefaultAsync();
        }
    }
}

using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class PointTransactionRepository : Repository<PointTransaction>, IPointTransactionRepository
    {
        public PointTransactionRepository(TechStoreContext context) : base(context) { }

        public async Task<IEnumerable<PointTransaction>> GetByUserIdAsync(Guid userId, int limit = 20)
        {
            return await _dbSet
                .Where(pt => pt.UserId == userId)
                .OrderByDescending(pt => pt.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetTotalPointsEarnedAsync(Guid userId)
        {
            return await _dbSet
                .Where(pt => pt.UserId == userId && pt.Type == "Earned")
                .SumAsync(pt => pt.Points);
        }

        public async Task<int> GetTotalPointsRedeemedAsync(Guid userId)
        {
            return await _dbSet
                .Where(pt => pt.UserId == userId && pt.Type == "Redeemed")
                .SumAsync(pt => Math.Abs(pt.Points));
        }
    }
}

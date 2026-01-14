using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    /// <summary>
    /// VoucherUsage repository implementation
    /// </summary>
    public class VoucherUsageRepository : Repository<VoucherUsage>, IVoucherUsageRepository
    {
        public VoucherUsageRepository(TechStoreContext context) : base(context)
        {
        }

        public async Task<IEnumerable<VoucherUsage>> GetByVoucherIdAsync(Guid voucherId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet
                .Include(vu => vu.User)
                .Include(vu => vu.Order)
                .AsQueryable();

            if (voucherId != Guid.Empty)
                query = query.Where(vu => vu.VoucherId == voucherId);

            if (startDate.HasValue)
                query = query.Where(vu => vu.UsedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(vu => vu.UsedAt <= endDate.Value);

            return await query.OrderByDescending(vu => vu.UsedAt).ToListAsync();
        }

        public async Task<IEnumerable<VoucherUsage>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(vu => vu.Voucher)
                .Include(vu => vu.Order)
                .Where(vu => vu.UserId == userId)
                .OrderByDescending(vu => vu.UsedAt)
                .ToListAsync();
        }
    }
}

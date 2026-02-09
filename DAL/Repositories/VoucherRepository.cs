using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    /// <summary>
    /// Voucher repository implementation
    /// </summary>
    public class VoucherRepository : Repository<Voucher>, IVoucherRepository
    {
        public VoucherRepository(TechStoreContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Voucher>> GetAllWithFiltersAsync(string? code = null, string? name = null, bool? isActive = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(code))
            {
                var codeLower = code.Trim().ToLower();
                query = query.Where(v => v.Code.ToLower().Contains(codeLower));
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                var nameLower = name.Trim().ToLower();
                query = query.Where(v => v.Name.ToLower().Contains(nameLower));
            }

            if (isActive.HasValue)
            {
                query = query.Where(v => v.IsActive == isActive.Value);
            }

            return await query.OrderByDescending(v => v.CreatedAt).ToListAsync();
        }

        public async Task<Voucher?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(v => v.Code == code);
        }

        public async Task<IEnumerable<Voucher>> GetActiveVouchersAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(v => v.IsActive 
                         && v.StartTime <= now 
                         && v.EndTime >= now)
                .ToListAsync();
        }

        public async Task<int> GetUsageCountAsync(Guid voucherId)
        {
            return await _context.Set<VoucherUsage>()
                .Where(vu => vu.VoucherId == voucherId)
                .CountAsync();
        }

        public async Task<int> GetUsageCountByUserAsync(Guid voucherId, Guid userId)
        {
            return await _context.Set<VoucherUsage>()
                .Where(vu => vu.VoucherId == voucherId && vu.UserId == userId)
                .CountAsync();
        }
    }
}

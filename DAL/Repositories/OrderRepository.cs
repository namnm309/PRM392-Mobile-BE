using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    /// <summary>
    /// Order repository implementation
    /// </summary>
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(TechStoreContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetByIdWithItemsAsync(Guid id)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Address)
                .Include(o => o.Voucher)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order?> GetByIdWithFullDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductImages)
                .Include(o => o.Address)
                .Include(o => o.Voucher)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<(IEnumerable<Order> Orders, int TotalCount)> SearchByOrderIdAsync(string orderIdSearch, int pageNumber, int pageSize)
        {
            var query = _dbSet.AsQueryable();

            // Search by order ID (partial match)
            if (Guid.TryParse(orderIdSearch, out var orderId))
            {
                query = query.Where(o => o.Id == orderId);
            }
            else
            {
                // If not a valid GUID, try to match as string (for partial search)
                var idString = orderIdSearch.ToLower();
                query = query.Where(o => o.Id.ToString().Contains(idString));
            }

            var totalCount = await query.CountAsync();

            var orders = await query
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Address)
                .Include(o => o.Voucher)
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }

        public async Task<(IEnumerable<Order> Orders, int TotalCount)> SearchByUserIdAsync(Guid userId, int pageNumber, int pageSize)
        {
            var query = _dbSet.Where(o => o.UserId == userId);

            var totalCount = await query.CountAsync();

            var orders = await query
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Address)
                .Include(o => o.Voucher)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }

        public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> CountByStatusAsync(string status, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.Where(o => o.Status == status);
            
            if (startDate.HasValue)
                query = query.Where(o => o.CreatedAt >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(o => o.CreatedAt <= endDate.Value);

            return await query.CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.Where(o => o.Status == "SUCCESS");
            
            if (startDate.HasValue)
                query = query.Where(o => o.CreatedAt >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(o => o.CreatedAt <= endDate.Value);

            return await query.SumAsync(o => o.TotalAmount);
        }
    }
}

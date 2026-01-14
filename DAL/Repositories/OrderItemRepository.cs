using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    /// <summary>
    /// OrderItem repository implementation
    /// </summary>
    public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(TechStoreContext context) : base(context)
        {
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId)
        {
            return await _dbSet
                .Include(oi => oi.Product)
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderItem>> GetByProductIdAndStatusAsync(Guid productId, string status)
        {
            return await _dbSet
                .Include(oi => oi.Order)
                .Where(oi => oi.ProductId == productId && oi.Status == status)
                .ToListAsync();
        }

        public async Task<bool> HasUserPurchasedProductAsync(Guid userId, Guid productId)
        {
            return await _dbSet
                .Include(oi => oi.Order)
                .AnyAsync(oi => oi.Order.UserId == userId 
                             && oi.ProductId == productId 
                             && oi.Status == "SUCCESS");
        }
    }
}

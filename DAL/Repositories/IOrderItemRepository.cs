using DAL.Models;

namespace DAL.Repositories
{
    /// <summary>
    /// OrderItem repository interface
    /// </summary>
    public interface IOrderItemRepository : IRepository<OrderItem>
    {
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId);
        Task<IEnumerable<OrderItem>> GetByProductIdAndStatusAsync(Guid productId, string status);
        Task<bool> HasUserPurchasedProductAsync(Guid userId, Guid productId);
    }
}

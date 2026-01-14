using DAL.Models;

namespace DAL.Repositories
{
    /// <summary>
    /// CartItem repository interface
    /// </summary>
    public interface ICartItemRepository : IRepository<CartItem>
    {
        Task<IEnumerable<CartItem>> GetByUserIdAsync(Guid userId);
        Task<CartItem?> GetByUserIdAndProductIdAsync(Guid userId, Guid productId);
        Task<CartItem?> GetByIdWithProductAsync(Guid id);
        Task<bool> ClearCartByUserIdAsync(Guid userId);
        Task<int> GetCartItemCountAsync(Guid userId);
    }
}

using DAL.Models;

namespace DAL.Repositories
{
    public interface IWishlistRepository : IRepository<WishlistItem>
    {
        Task<IEnumerable<WishlistItem>> GetByUserIdAsync(Guid userId);
        Task<WishlistItem?> GetByUserIdAndProductIdAsync(Guid userId, Guid productId);
        Task<bool> IsProductInWishlistAsync(Guid userId, Guid productId);
        Task<int> GetWishlistCountAsync(Guid userId);
    }
}

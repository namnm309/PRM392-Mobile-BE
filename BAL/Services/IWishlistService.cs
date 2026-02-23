using BAL.DTOs.Wishlist;

namespace BAL.Services
{
    public interface IWishlistService
    {
        Task<IEnumerable<WishlistItemDto>> GetWishlistAsync(Guid userId);
        Task<WishlistItemDto> AddToWishlistAsync(Guid userId, Guid productId);
        Task<bool> RemoveFromWishlistAsync(Guid userId, Guid productId);
        Task<WishlistStatusDto> GetWishlistStatusAsync(Guid userId, Guid productId);
        Task<int> GetWishlistCountAsync(Guid userId);
    }
}

using BAL.DTOs.Wishlist;
using DAL.Models;
using DAL.Repositories;
using Microsoft.Extensions.Logging;

namespace BAL.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<WishlistService> _logger;

        public WishlistService(
            IWishlistRepository wishlistRepository,
            IProductRepository productRepository,
            ILogger<WishlistService> logger)
        {
            _wishlistRepository = wishlistRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<WishlistItemDto>> GetWishlistAsync(Guid userId)
        {
            var items = await _wishlistRepository.GetByUserIdAsync(userId);
            return items.Select(MapToDto);
        }

        public async Task<WishlistItemDto> AddToWishlistAsync(Guid userId, Guid productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {productId} not found");

            var existing = await _wishlistRepository.GetByUserIdAndProductIdAsync(userId, productId);
            if (existing != null)
                throw new InvalidOperationException("Product is already in your wishlist");

            var item = new WishlistItem
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };

            await _wishlistRepository.AddAsync(item);
            _logger.LogInformation("User {UserId} added product {ProductId} to wishlist", userId, productId);

            var saved = await _wishlistRepository.GetByUserIdAndProductIdAsync(userId, productId);
            return MapToDto(saved!);
        }

        public async Task<bool> RemoveFromWishlistAsync(Guid userId, Guid productId)
        {
            var item = await _wishlistRepository.GetByUserIdAndProductIdAsync(userId, productId);
            if (item == null) return false;

            await _wishlistRepository.DeleteAsync(item.Id);
            _logger.LogInformation("User {UserId} removed product {ProductId} from wishlist", userId, productId);
            return true;
        }

        public async Task<WishlistStatusDto> GetWishlistStatusAsync(Guid userId, Guid productId)
        {
            var item = await _wishlistRepository.GetByUserIdAndProductIdAsync(userId, productId);
            return new WishlistStatusDto
            {
                IsInWishlist = item != null,
                AddedAt = item?.CreatedAt
            };
        }

        public async Task<int> GetWishlistCountAsync(Guid userId)
        {
            return await _wishlistRepository.GetWishlistCountAsync(userId);
        }

        private WishlistItemDto MapToDto(WishlistItem item)
        {
            var imageUrl = item.Product?.ProductImages?.OrderBy(pi => pi.DisplayOrder).FirstOrDefault()?.ImageUrl;

            return new WishlistItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? string.Empty,
                ProductImageUrl = imageUrl,
                ProductPrice = item.Product?.Price ?? 0,
                ProductDiscountPrice = item.Product?.DiscountPrice,
                IsOnSale = item.Product?.IsOnSale ?? false,
                IsAvailable = item.Product?.IsActive == true && item.Product.Stock > 0,
                Stock = item.Product?.Stock ?? 0,
                AddedAt = item.CreatedAt
            };
        }
    }
}

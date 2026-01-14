using BAL.DTOs.Cart;
using DAL.Models;
using DAL.Repositories;

namespace BAL.Services
{
    /// <summary>
    /// Cart service implementation with business logic
    /// </summary>
    public class CartService : ICartService
    {
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;

        public CartService(ICartItemRepository cartItemRepository, IProductRepository productRepository)
        {
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
        }

        public async Task<CartResponseDto> GetCartAsync(Guid userId)
        {
            var cartItems = await _cartItemRepository.GetByUserIdAsync(userId);
            var items = new List<CartItemResponseDto>();

            foreach (var item in cartItems)
            {
                var itemDto = await MapToDtoWithStatusAsync(item);
                items.Add(itemDto);
            }

            var totalAmount = items
                .Where(i => i.IsAvailable)
                .Sum(i => (i.ProductDiscountPrice ?? i.ProductPrice) * i.Quantity);

            return new CartResponseDto
            {
                Items = items,
                TotalItems = items.Sum(i => i.Quantity),
                TotalAmount = totalAmount
            };
        }

        public async Task<CartItemResponseDto> AddItemAsync(Guid userId, AddCartItemRequestDto request)
        {
            // Business rule: Check if product exists, is active, and has stock
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
                throw new InvalidOperationException("Product not found");

            if (!product.IsActive)
                throw new InvalidOperationException("Product is inactive");

            if (product.Stock < request.Quantity)
                throw new InvalidOperationException($"Insufficient stock. Available: {product.Stock}");

            // Business rule: Check if item already exists in cart
            var existingItem = await _cartItemRepository.GetByUserIdAndProductIdAsync(userId, request.ProductId);
            
            if (existingItem != null)
            {
                // Update quantity
                var newQuantity = existingItem.Quantity + request.Quantity;
                if (newQuantity > product.Stock)
                    throw new InvalidOperationException($"Cannot add more items. Max available: {product.Stock}");

                existingItem.Quantity = newQuantity;
                existingItem.UnitPriceSnapshot = product.DiscountPrice ?? product.Price;
                existingItem.UpdatedAt = DateTime.UtcNow;
                var updatedItem = await _cartItemRepository.UpdateAsync(existingItem);
                return await MapToDtoWithStatusAsync(updatedItem);
            }
            else
            {
                // Create new cart item
                var cartItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    UnitPriceSnapshot = product.DiscountPrice ?? product.Price,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdItem = await _cartItemRepository.AddAsync(cartItem);
                return await MapToDtoWithStatusAsync(createdItem);
            }
        }

        public async Task<CartItemResponseDto?> UpdateItemQuantityAsync(Guid userId, Guid itemId, UpdateCartItemRequestDto request)
        {
            var cartItem = await _cartItemRepository.GetByIdWithProductAsync(itemId);
            if (cartItem == null || cartItem.UserId != userId)
                return null;

            // Business rule: Validate product availability
            var product = cartItem.Product ?? await _productRepository.GetByIdAsync(cartItem.ProductId);
            if (product == null)
                throw new InvalidOperationException("Product not found");

            if (!product.IsActive)
                throw new InvalidOperationException("Product is inactive");

            if (product.Stock < request.Quantity)
                throw new InvalidOperationException($"Insufficient stock. Available: {product.Stock}");

            cartItem.Quantity = request.Quantity;
            cartItem.UnitPriceSnapshot = product.DiscountPrice ?? product.Price;
            cartItem.UpdatedAt = DateTime.UtcNow;
            var updatedItem = await _cartItemRepository.UpdateAsync(cartItem);
            return await MapToDtoWithStatusAsync(updatedItem);
        }

        public async Task<bool> RemoveItemAsync(Guid userId, Guid itemId)
        {
            var cartItem = await _cartItemRepository.GetByIdAsync(itemId);
            if (cartItem == null || cartItem.UserId != userId)
                return false;

            return await _cartItemRepository.DeleteAsync(itemId);
        }

        public async Task<bool> ClearCartAsync(Guid userId)
        {
            return await _cartItemRepository.ClearCartByUserIdAsync(userId);
        }

        public async Task<bool> ValidateCartForCheckoutAsync(Guid userId)
        {
            var cartItems = await _cartItemRepository.GetByUserIdAsync(userId);
            
            foreach (var item in cartItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                
                if (product == null)
                    return false; // Product not found

                if (!product.IsActive)
                    return false; // Product inactive

                if (product.Stock < item.Quantity)
                    return false; // Insufficient stock
            }

            return true;
        }

        private Task<CartItemResponseDto> MapToDtoWithStatusAsync(CartItem item)
        {
            // Product should be loaded via Include in repository
            var product = item.Product;
            
            var dto = new CartItemResponseDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            };

            if (product == null)
            {
                // If product is not loaded, fetch it
                return MapToDtoWithStatusAsyncInternal(item);
            }

            dto.ProductName = product.Name;
            dto.ProductPrice = product.Price;
            dto.ProductDiscountPrice = product.DiscountPrice;
            dto.ProductImageUrl = product.ImageUrl;

            if (!product.IsActive)
            {
                dto.IsAvailable = false;
                dto.ReasonUnavailable = "INACTIVE";
                dto.MaxQuantity = 0;
            }
            else if (product.Stock <= 0)
            {
                dto.IsAvailable = false;
                dto.ReasonUnavailable = "OUT_OF_STOCK";
                dto.MaxQuantity = 0;
            }
            else
            {
                dto.IsAvailable = true;
                dto.MaxQuantity = product.Stock;
                dto.ReasonUnavailable = null;
            }

            return Task.FromResult(dto);
        }

        private async Task<CartItemResponseDto> MapToDtoWithStatusAsyncInternal(CartItem item)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            
            var dto = new CartItemResponseDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            };

            if (product == null)
            {
                dto.IsAvailable = false;
                dto.ReasonUnavailable = "NOT_FOUND";
                dto.MaxQuantity = 0;
                dto.ProductName = "Product not found";
                dto.ProductPrice = 0;
            }
            else
            {
                dto.ProductName = product.Name;
                dto.ProductPrice = product.Price;
                dto.ProductDiscountPrice = product.DiscountPrice;
                dto.ProductImageUrl = product.ImageUrl;

                if (!product.IsActive)
                {
                    dto.IsAvailable = false;
                    dto.ReasonUnavailable = "INACTIVE";
                    dto.MaxQuantity = 0;
                }
                else if (product.Stock <= 0)
                {
                    dto.IsAvailable = false;
                    dto.ReasonUnavailable = "OUT_OF_STOCK";
                    dto.MaxQuantity = 0;
                }
                else
                {
                    dto.IsAvailable = true;
                    dto.MaxQuantity = product.Stock;
                    dto.ReasonUnavailable = null;
                }
            }

            return dto;
        }
    }
}

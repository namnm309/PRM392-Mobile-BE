using BAL.DTOs.Cart;
using DAL.Models;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BAL.Services
{
    /// <summary>
    /// Cart service implementation with business logic
    /// </summary>
    public class CartService : ICartService
    {
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly DAL.Data.TechStoreContext _context;

        public CartService(ICartItemRepository cartItemRepository, IProductRepository productRepository, DAL.Data.TechStoreContext context)
        {
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _context = context;
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

            ProductVariant? variant = null;
            if (request.VariantId.HasValue)
            {
                variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(v => v.Id == request.VariantId.Value && v.ProductId == request.ProductId);
                if (variant == null)
                    throw new InvalidOperationException("Variant not found");
                if (!variant.IsActive)
                    throw new InvalidOperationException("Variant is inactive");
                if (variant.Stock < request.Quantity)
                    throw new InvalidOperationException($"Insufficient stock. Available: {variant.Stock}");
            }
            else
            {
                if (product.Stock < request.Quantity)
                    throw new InvalidOperationException($"Insufficient stock. Available: {product.Stock}");
            }

            // Business rule: Check if item already exists in cart
            var existingItem = await _cartItemRepository.GetByUserIdAndProductIdAsync(userId, request.ProductId, request.VariantId);
            
            if (existingItem != null)
            {
                // Update quantity
                var newQuantity = existingItem.Quantity + request.Quantity;
                var maxStock = variant != null ? variant.Stock : product.Stock;
                if (newQuantity > maxStock)
                    throw new InvalidOperationException($"Cannot add more items. Max available: {maxStock}");

                existingItem.Quantity = newQuantity;
                existingItem.UnitPriceSnapshot = variant != null
                    ? (variant.DiscountPrice ?? variant.Price)
                    : (product.DiscountPrice ?? product.Price);
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
                    VariantId = request.VariantId,
                    Quantity = request.Quantity,
                    UnitPriceSnapshot = variant != null
                        ? (variant.DiscountPrice ?? variant.Price)
                        : (product.DiscountPrice ?? product.Price),
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

            ProductVariant? variant = null;
            if (cartItem.VariantId.HasValue)
            {
                variant = cartItem.Variant ?? await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == cartItem.VariantId.Value);
                if (variant == null)
                    throw new InvalidOperationException("Variant not found");
                if (!variant.IsActive)
                    throw new InvalidOperationException("Variant is inactive");
                if (variant.Stock < request.Quantity)
                    throw new InvalidOperationException($"Insufficient stock. Available: {variant.Stock}");
            }
            else
            {
                if (product.Stock < request.Quantity)
                    throw new InvalidOperationException($"Insufficient stock. Available: {product.Stock}");
            }

            cartItem.Quantity = request.Quantity;
            cartItem.UnitPriceSnapshot = variant != null
                ? (variant.DiscountPrice ?? variant.Price)
                : (product.DiscountPrice ?? product.Price);
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

                if (item.VariantId.HasValue)
                {
                    var variant = item.Variant ?? await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == item.VariantId.Value);
                    if (variant == null || !variant.IsActive)
                        return false;
                    if (variant.Stock < item.Quantity)
                        return false; // Insufficient stock
                }
                else
                {
                    if (product.Stock < item.Quantity)
                        return false; // Insufficient stock
                }
            }

            return true;
        }

        private Task<CartItemResponseDto> MapToDtoWithStatusAsync(CartItem item)
        {
            // Product should be loaded via Include in repository
            var product = item.Product;
            var variant = item.Variant;
            
            var dto = new CartItemResponseDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                VariantId = item.VariantId,
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
            dto.ProductPrice = variant != null ? variant.Price : product.Price;
            dto.ProductDiscountPrice = variant != null ? variant.DiscountPrice : product.DiscountPrice;
            dto.ProductImageUrl = product.ImageUrl;
            if (variant != null)
            {
                dto.VariantColorName = variant.ColorName;
                dto.VariantColorHex = variant.ColorHex;
                dto.VariantRamGb = variant.RamGb;
                dto.VariantStorageGb = variant.StorageGb;
            }

            if (!product.IsActive)
            {
                dto.IsAvailable = false;
                dto.ReasonUnavailable = "INACTIVE";
                dto.MaxQuantity = 0;
            }
            else if (variant != null && !variant.IsActive)
            {
                dto.IsAvailable = false;
                dto.ReasonUnavailable = "INACTIVE";
                dto.MaxQuantity = 0;
            }
            else if (variant != null && variant.Stock <= 0)
            {
                dto.IsAvailable = false;
                dto.ReasonUnavailable = "OUT_OF_STOCK";
                dto.MaxQuantity = 0;
            }
            else if (variant == null && product.Stock <= 0)
            {
                dto.IsAvailable = false;
                dto.ReasonUnavailable = "OUT_OF_STOCK";
                dto.MaxQuantity = 0;
            }
            else
            {
                dto.IsAvailable = true;
                dto.MaxQuantity = variant != null ? variant.Stock : product.Stock;
                dto.ReasonUnavailable = null;
            }

            return Task.FromResult(dto);
        }

        private async Task<CartItemResponseDto> MapToDtoWithStatusAsyncInternal(CartItem item)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            ProductVariant? variant = null;
            if (item.VariantId.HasValue)
            {
                variant = await _context.ProductVariants.FirstOrDefaultAsync(v => v.Id == item.VariantId.Value);
            }
            
            var dto = new CartItemResponseDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                VariantId = item.VariantId,
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
                dto.ProductPrice = variant != null ? variant.Price : product.Price;
                dto.ProductDiscountPrice = variant != null ? variant.DiscountPrice : product.DiscountPrice;
                dto.ProductImageUrl = product.ImageUrl;
                if (variant != null)
                {
                    dto.VariantColorName = variant.ColorName;
                    dto.VariantColorHex = variant.ColorHex;
                    dto.VariantRamGb = variant.RamGb;
                    dto.VariantStorageGb = variant.StorageGb;
                }

                if (!product.IsActive)
                {
                    dto.IsAvailable = false;
                    dto.ReasonUnavailable = "INACTIVE";
                    dto.MaxQuantity = 0;
                }
                else if (variant != null && !variant.IsActive)
                {
                    dto.IsAvailable = false;
                    dto.ReasonUnavailable = "INACTIVE";
                    dto.MaxQuantity = 0;
                }
                else if (variant != null && variant.Stock <= 0)
                {
                    dto.IsAvailable = false;
                    dto.ReasonUnavailable = "OUT_OF_STOCK";
                    dto.MaxQuantity = 0;
                }
                else if (variant == null && product.Stock <= 0)
                {
                    dto.IsAvailable = false;
                    dto.ReasonUnavailable = "OUT_OF_STOCK";
                    dto.MaxQuantity = 0;
                }
                else
                {
                    dto.IsAvailable = true;
                    dto.MaxQuantity = variant != null ? variant.Stock : product.Stock;
                    dto.ReasonUnavailable = null;
                }
            }

            return dto;
        }
    }
}

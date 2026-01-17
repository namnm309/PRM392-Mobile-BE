using BAL.DTOs.Cart;

namespace BAL.Services
{
    /// <summary>
    /// Cart service interface
    /// </summary>
    public interface ICartService
    {
        Task<CartResponseDto> GetCartAsync(Guid userId);
        Task<CartItemResponseDto> AddItemAsync(Guid userId, AddCartItemRequestDto request);
        Task<CartItemResponseDto?> UpdateItemQuantityAsync(Guid userId, Guid itemId, UpdateCartItemRequestDto request);
        Task<bool> RemoveItemAsync(Guid userId, Guid itemId);
        Task<bool> ClearCartAsync(Guid userId);
        Task<bool> ValidateCartForCheckoutAsync(Guid userId);
    }
}

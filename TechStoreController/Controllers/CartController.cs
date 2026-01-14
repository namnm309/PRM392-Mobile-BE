using BAL.DTOs.Cart;
using BAL.DTOs.Common;
using BAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStoreController.Helpers;

namespace TechStoreController.Controllers
{
    /// <summary>
    /// Cart API Controller - Customer only
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        /// <summary>
        /// Get current user's cart with availability status
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<CartResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<CartResponseDto>>> GetCart()
        {
            try
            {
                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<CartResponseDto>.ErrorResponse("User not authenticated"));

                var cart = await _cartService.GetCartAsync(userId.Value);
                return Ok(ApiResponse<CartResponseDto>.SuccessResponse(cart, "Cart retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart");
                return StatusCode(500, ApiResponse<CartResponseDto>.ErrorResponse("An error occurred while retrieving the cart"));
            }
        }

        /// <summary>
        /// Add item to cart
        /// </summary>
        [HttpPost("items")]
        [ProducesResponseType(typeof(ApiResponse<CartItemResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<CartItemResponseDto>>> AddItem([FromBody] AddCartItemRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<CartItemResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<CartItemResponseDto>.ErrorResponse("User not authenticated"));

                var item = await _cartService.AddItemAsync(userId.Value, request);
                return CreatedAtAction(
                    nameof(GetCart),
                    null,
                    ApiResponse<CartItemResponseDto>.SuccessResponse(item, "Item added to cart successfully")
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<CartItemResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return StatusCode(500, ApiResponse<CartItemResponseDto>.ErrorResponse("An error occurred while adding item to cart"));
            }
        }

        /// <summary>
        /// Update cart item quantity
        /// </summary>
        [HttpPut("items/{itemId}")]
        [ProducesResponseType(typeof(ApiResponse<CartItemResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<CartItemResponseDto>>> UpdateItemQuantity(Guid itemId, [FromBody] UpdateCartItemRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<CartItemResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<CartItemResponseDto>.ErrorResponse("User not authenticated"));

                var item = await _cartService.UpdateItemQuantityAsync(userId.Value, itemId, request);
                if (item == null)
                    return NotFound(ApiResponse<CartItemResponseDto>.ErrorResponse("Cart item not found"));

                return Ok(ApiResponse<CartItemResponseDto>.SuccessResponse(item, "Cart item updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<CartItemResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item {ItemId}", itemId);
                return StatusCode(500, ApiResponse<CartItemResponseDto>.ErrorResponse("An error occurred while updating cart item"));
            }
        }

        /// <summary>
        /// Remove item from cart
        /// </summary>
        [HttpDelete("items/{itemId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> RemoveItem(Guid itemId)
        {
            try
            {
                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

                var result = await _cartService.RemoveItemAsync(userId.Value, itemId);
                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResponse("Cart item not found"));

                return Ok(ApiResponse<object?>.SuccessResponse(null, "Item removed from cart successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cart item {ItemId}", itemId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while removing cart item"));
            }
        }

        /// <summary>
        /// Clear entire cart
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> ClearCart()
        {
            try
            {
                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

                await _cartService.ClearCartAsync(userId.Value);
                return Ok(ApiResponse<object?>.SuccessResponse(null, "Cart cleared successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while clearing cart"));
            }
        }

        /// <summary>
        /// Validate cart for checkout
        /// </summary>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<bool>>> ValidateCart()
        {
            try
            {
                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));

                var isValid = await _cartService.ValidateCartForCheckoutAsync(userId.Value);
                return Ok(ApiResponse<bool>.SuccessResponse(isValid, isValid ? "Cart is valid for checkout" : "Cart contains invalid items"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating cart");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while validating cart"));
            }
        }
    }
}

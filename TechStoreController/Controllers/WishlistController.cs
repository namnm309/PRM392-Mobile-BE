using BAL.DTOs.Common;
using BAL.DTOs.Wishlist;
using BAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStoreController.Helpers;

namespace TechStoreController.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;
        private readonly ILogger<WishlistController> _logger;

        public WishlistController(IWishlistService wishlistService, ILogger<WishlistController> logger)
        {
            _wishlistService = wishlistService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<WishlistItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<WishlistItemDto>>>> GetWishlist()
        {
            var userId = JwtHelper.GetUserId(User);
            if (userId == null)
                return Unauthorized(ApiResponse<IEnumerable<WishlistItemDto>>.ErrorResponse("User not authenticated"));

            var wishlist = await _wishlistService.GetWishlistAsync(userId.Value);
            return Ok(ApiResponse<IEnumerable<WishlistItemDto>>.SuccessResponse(wishlist, "Wishlist retrieved successfully"));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WishlistItemDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<WishlistItemDto>>> AddToWishlist([FromBody] AddToWishlistRequest request)
        {
            var userId = JwtHelper.GetUserId(User);
            if (userId == null)
                return Unauthorized(ApiResponse<WishlistItemDto>.ErrorResponse("User not authenticated"));

            try
            {
                var item = await _wishlistService.AddToWishlistAsync(userId.Value, request.ProductId);
                return CreatedAtAction(nameof(GetWishlist), null, ApiResponse<WishlistItemDto>.SuccessResponse(item, "Product added to wishlist"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<WishlistItemDto>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<WishlistItemDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{productId}")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object?>>> RemoveFromWishlist(Guid productId)
        {
            var userId = JwtHelper.GetUserId(User);
            if (userId == null)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            var result = await _wishlistService.RemoveFromWishlistAsync(userId.Value, productId);
            if (!result)
                return NotFound(ApiResponse<object>.ErrorResponse("Product not found in wishlist"));

            return Ok(ApiResponse<object?>.SuccessResponse(null, "Product removed from wishlist"));
        }

        [HttpGet("status/{productId}")]
        [ProducesResponseType(typeof(ApiResponse<WishlistStatusDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<WishlistStatusDto>>> GetWishlistStatus(Guid productId)
        {
            var userId = JwtHelper.GetUserId(User);
            if (userId == null)
                return Unauthorized(ApiResponse<WishlistStatusDto>.ErrorResponse("User not authenticated"));

            var status = await _wishlistService.GetWishlistStatusAsync(userId.Value, productId);
            return Ok(ApiResponse<WishlistStatusDto>.SuccessResponse(status, "Status retrieved"));
        }

        [HttpGet("count")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<int>>> GetWishlistCount()
        {
            var userId = JwtHelper.GetUserId(User);
            if (userId == null)
                return Unauthorized(ApiResponse<int>.ErrorResponse("User not authenticated"));

            var count = await _wishlistService.GetWishlistCountAsync(userId.Value);
            return Ok(ApiResponse<int>.SuccessResponse(count, "Count retrieved"));
        }
    }
}

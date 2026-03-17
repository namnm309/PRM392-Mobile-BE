using BAL.DTOs.Common;
using BAL.DTOs.Review;
using BAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStoreController.Helpers;

namespace TechStoreController.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReviewResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ReviewResponseDto>>>> GetReviewsByProduct(Guid productId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByProductIdAsync(productId);
                return Ok(ApiResponse<IEnumerable<ReviewResponseDto>>.SuccessResponse(reviews, "Reviews retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for product {ProductId}", productId);
                return StatusCode(500, ApiResponse<IEnumerable<ReviewResponseDto>>.ErrorResponse("An error occurred while retrieving reviews"));
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ReviewResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<ReviewResponseDto>>> CreateReview([FromBody] CreateReviewRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<ReviewResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<ReviewResponseDto>.ErrorResponse("User not authenticated"));

                var review = await _reviewService.CreateReviewAsync(userId.Value, request);
                return CreatedAtAction(
                    nameof(GetReviewsByProduct),
                    new { productId = review.ProductId },
                    ApiResponse<ReviewResponseDto>.SuccessResponse(review, "Review created successfully")
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<ReviewResponseDto>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<ReviewResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                return StatusCode(500, ApiResponse<ReviewResponseDto>.ErrorResponse("An error occurred while creating review"));
            }
        }

        [HttpPost("{reviewId}/reply")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ReviewReplyResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<ReviewReplyResponseDto>>> ReplyToReview(Guid reviewId, [FromBody] CreateReviewReplyRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<ReviewReplyResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var staffId = JwtHelper.GetUserId(User);
                if (staffId == null)
                    return Unauthorized(ApiResponse<ReviewReplyResponseDto>.ErrorResponse("User not authenticated"));

                var reply = await _reviewService.ReplyToReviewAsync(reviewId, staffId.Value, request);
                return CreatedAtAction(
                    nameof(GetReviewsByProduct),
                    null,
                    ApiResponse<ReviewReplyResponseDto>.SuccessResponse(reply, "Reply created successfully")
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<ReviewReplyResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replying to review {ReviewId}", reviewId);
                return StatusCode(500, ApiResponse<ReviewReplyResponseDto>.ErrorResponse("An error occurred while replying to review"));
            }
        }

        [HttpDelete("{reviewId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteReview(Guid reviewId)
        {
            try
            {
                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

                var userRole = JwtHelper.GetUserRole(User);
                if (userRole != "Admin")
                    return Forbid();

                var result = await _reviewService.DeleteReviewAsync(reviewId);
                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResponse("Review not found"));

                return Ok(ApiResponse<object?>.SuccessResponse(null, "Review deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId}", reviewId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while deleting review"));
            }
        }
    }
}

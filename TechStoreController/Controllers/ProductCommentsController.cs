using BAL.DTOs.Common;
using BAL.DTOs.ProductComment;
using BAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStoreController.Helpers;

namespace TechStoreController.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductCommentsController : ControllerBase
    {
        private readonly IProductCommentService _commentService;
        private readonly ILogger<ProductCommentsController> _logger;

        public ProductCommentsController(IProductCommentService commentService, ILogger<ProductCommentsController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        /// <summary>
        /// Get all comments (Q&amp;A) for a product, returned as a nested tree
        /// </summary>
        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductCommentResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductCommentResponseDto>>>> GetCommentsByProduct(Guid productId)
        {
            try
            {
                var comments = await _commentService.GetCommentsByProductIdAsync(productId);
                return Ok(ApiResponse<IEnumerable<ProductCommentResponseDto>>.SuccessResponse(comments, "Comments retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments for product {ProductId}", productId);
                return StatusCode(500, ApiResponse<IEnumerable<ProductCommentResponseDto>>.ErrorResponse("An error occurred while retrieving comments"));
            }
        }

        /// <summary>
        /// Create a new comment or reply to an existing comment.
        /// Set parentId to reply to a specific comment; leave null for a top-level comment.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ProductCommentResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<ProductCommentResponseDto>>> CreateComment([FromBody] CreateProductCommentRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<ProductCommentResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<ProductCommentResponseDto>.ErrorResponse("User not authenticated"));

                var comment = await _commentService.CreateCommentAsync(userId.Value, request);
                return CreatedAtAction(
                    nameof(GetCommentsByProduct),
                    new { productId = comment.ProductId },
                    ApiResponse<ProductCommentResponseDto>.SuccessResponse(comment, "Comment created successfully")
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<ProductCommentResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment");
                return StatusCode(500, ApiResponse<ProductCommentResponseDto>.ErrorResponse("An error occurred while creating comment"));
            }
        }

        /// <summary>
        /// Delete a comment and all its nested replies. Admin only.
        /// </summary>
        [HttpDelete("{commentId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteComment(Guid commentId)
        {
            try
            {
                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

                var userRole = JwtHelper.GetUserRole(User);
                if (userRole != "Admin")
                    return Forbid();

                var result = await _commentService.DeleteCommentAsync(commentId);
                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResponse("Comment not found"));

                return Ok(ApiResponse<object?>.SuccessResponse(null, "Comment deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while deleting comment"));
            }
        }
    }
}

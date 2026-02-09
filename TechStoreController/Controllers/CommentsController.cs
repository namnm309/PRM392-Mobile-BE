using BAL.DTOs.Comment;
using BAL.DTOs.Common;
using BAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStoreController.Helpers;

namespace TechStoreController.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(ICommentService commentService, ILogger<CommentsController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CommentResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CommentResponseDto>>>> GetCommentsByProduct(Guid productId)
        {
            try
            {
                var comments = await _commentService.GetCommentsByProductIdAsync(productId);
                return Ok(ApiResponse<IEnumerable<CommentResponseDto>>.SuccessResponse(comments, "Comments retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments for product {ProductId}", productId);
                return StatusCode(500, ApiResponse<IEnumerable<CommentResponseDto>>.ErrorResponse("An error occurred while retrieving comments"));
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<CommentResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<CommentResponseDto>>> CreateComment([FromBody] CreateCommentRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<CommentResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<CommentResponseDto>.ErrorResponse("User not authenticated"));

                var comment = await _commentService.CreateCommentAsync(userId.Value, request);
                return CreatedAtAction(
                    nameof(GetCommentsByProduct),
                    new { productId = comment.ProductId },
                    ApiResponse<CommentResponseDto>.SuccessResponse(comment, "Comment created successfully")
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<CommentResponseDto>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<CommentResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment");
                return StatusCode(500, ApiResponse<CommentResponseDto>.ErrorResponse("An error occurred while creating comment"));
            }
        }

        [HttpPost("{commentId}/reply")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<CommentReplyResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<CommentReplyResponseDto>>> ReplyToComment(Guid commentId, [FromBody] CreateCommentReplyRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<CommentReplyResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var staffId = JwtHelper.GetUserId(User);
                if (staffId == null)
                    return Unauthorized(ApiResponse<CommentReplyResponseDto>.ErrorResponse("User not authenticated"));

                var reply = await _commentService.ReplyToCommentAsync(commentId, staffId.Value, request);
                return CreatedAtAction(
                    nameof(GetCommentsByProduct),
                    null,
                    ApiResponse<CommentReplyResponseDto>.SuccessResponse(reply, "Reply created successfully")
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<CommentReplyResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replying to comment {CommentId}", commentId);
                return StatusCode(500, ApiResponse<CommentReplyResponseDto>.ErrorResponse("An error occurred while replying to comment"));
            }
        }
    }
}

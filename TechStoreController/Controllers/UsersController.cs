using BAL.DTOs.Common;
using BAL.DTOs.User;
using BAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStoreController.Helpers;

namespace TechStoreController.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    //[Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }


        /// <summary>
        /// Who Am I
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetMe()
        {
            var userId = JwtHelper.GetUserId(User);
            if (userId == null)
                return Unauthorized(ApiResponse<UserResponseDto>.ErrorResponse("User not authenticated"));

            try
            {
                var user = await _userService.GetUserByIdAsync(userId.Value);
                if (user == null)
                    return NotFound(ApiResponse<UserResponseDto>.ErrorResponse("User not found"));

                return Ok(ApiResponse<UserResponseDto>.SuccessResponse(user, "User retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user");
                return StatusCode(500, ApiResponse<UserResponseDto>.ErrorResponse("An error occurred while retrieving the user"));
            }
        }

        /// <summary>
        /// Who Is That (Staff/Admin)
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetUserById(Guid id)
        {
            var currentUserId = JwtHelper.GetUserId(User);
            if (currentUserId == null)
                return Unauthorized(ApiResponse<UserResponseDto>.ErrorResponse("User not authenticated"));

            if (currentUserId != id && !JwtHelper.HasRole(User, JwtHelper.RoleStaff, JwtHelper.RoleAdmin))
                return StatusCode(403, ApiResponse<UserResponseDto>.ErrorResponse("Forbidden"));

            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(ApiResponse<UserResponseDto>.ErrorResponse("User not found"));

                return Ok(ApiResponse<UserResponseDto>.SuccessResponse(user, "User retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
                return StatusCode(500, ApiResponse<UserResponseDto>.ErrorResponse("An error occurred while retrieving the user"));
            }
        }

        /// <summary>
        /// Tìm user = Clerk ID / chỉ test cho clerk dashboard
        /// </summary>
        [HttpGet("clerk/{clerkId}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetUserByClerkId(string clerkId)
        {
            var currentClerkId = JwtHelper.GetClerkId(User);
            if (string.IsNullOrEmpty(currentClerkId))
                return Unauthorized(ApiResponse<UserResponseDto>.ErrorResponse("User not authenticated"));

            if (currentClerkId != clerkId && !JwtHelper.HasRole(User, JwtHelper.RoleStaff, JwtHelper.RoleAdmin))
                return StatusCode(403, ApiResponse<UserResponseDto>.ErrorResponse("Forbidden"));

            try
            {
                var user = await _userService.GetUserByClerkIdAsync(clerkId);
                if (user == null)
                    return NotFound(ApiResponse<UserResponseDto>.ErrorResponse("User not found"));

                return Ok(ApiResponse<UserResponseDto>.SuccessResponse(user, "User retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ClerkId: {ClerkId}", clerkId);
                return StatusCode(500, ApiResponse<UserResponseDto>.ErrorResponse("An error occurred while retrieving the user"));
            }
        }

        /// <summary>
        /// Tạo user mới ( sau khi đăng ký = clerk , ko xài ) 
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> CreateUser([FromBody] CreateUserRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(ApiResponse<UserResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var user = await _userService.CreateUserAsync(request);
                return CreatedAtAction(
                    nameof(GetUserById),
                    new { id = user.Id },
                    ApiResponse<UserResponseDto>.SuccessResponse(user, "User created successfully")
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<UserResponseDto>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<UserResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, ApiResponse<UserResponseDto>.ErrorResponse("An error occurred while creating the user"));
            }
        }

        /// <summary>
        /// Update my self ( just myself)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> UpdateUser(Guid id, [FromBody] UpdateUserRequestDto? request)
        {
            var currentUserId = JwtHelper.GetUserId(User);
            if (currentUserId == null)
                return Unauthorized(ApiResponse<UserResponseDto>.ErrorResponse("User not authenticated"));

            if (currentUserId != id)
                return StatusCode(403, ApiResponse<UserResponseDto>.ErrorResponse("Forbidden"));

            if (request == null)
                return BadRequest(ApiResponse<UserResponseDto>.ErrorResponse("Request body is required and must be valid JSON."));

            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse<UserResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var user = await _userService.UpdateUserAsync(id, request);
                if (user == null)
                    return NotFound(ApiResponse<UserResponseDto>.ErrorResponse("User not found"));

                return Ok(ApiResponse<UserResponseDto>.SuccessResponse(user, "User updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Update user validation/db error for ID: {UserId}", id);
                return BadRequest(ApiResponse<UserResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
                var message = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, ApiResponse<UserResponseDto>.ErrorResponse($"An error occurred while updating the user: {message}"));
            }
        }

        /// <summary>
        /// Delete user / soft delete (only own user or Admin).
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(Guid id)
        {
            var currentUserId = JwtHelper.GetUserId(User);
            if (currentUserId == null)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            if (currentUserId != id && !JwtHelper.HasRole(User, JwtHelper.RoleAdmin))
                return StatusCode(403, ApiResponse<object>.ErrorResponse("Forbidden"));

            try
            {
                var result = await _userService.DeleteUserAsync(id);
                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResponse("User not found"));

                return Ok(ApiResponse<object?>.SuccessResponse(null, "User deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while deleting the user"));
            }
        }
    }
}

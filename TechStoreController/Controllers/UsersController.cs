using BAL.DTOs.Common;
using BAL.DTOs.User;
using BAL.Services;
using Microsoft.AspNetCore.Mvc;

namespace TechStoreController.Controllers
{
    /// <summary>
    /// Users API Controller - API Layer
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
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
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetUserById(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                
                if (user == null)
                {
                    return NotFound(ApiResponse<UserResponseDto>.ErrorResponse("User not found"));
                }

                return Ok(ApiResponse<UserResponseDto>.SuccessResponse(user, "User retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
                return StatusCode(500, ApiResponse<UserResponseDto>.ErrorResponse("An error occurred while retrieving the user"));
            }
        }

        /// <summary>
        /// Get user by Clerk ID
        /// </summary>
        [HttpGet("clerk/{clerkId}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetUserByClerkId(string clerkId)
        {
            try
            {
                var user = await _userService.GetUserByClerkIdAsync(clerkId);
                
                if (user == null)
                {
                    return NotFound(ApiResponse<UserResponseDto>.ErrorResponse("User not found"));
                }

                return Ok(ApiResponse<UserResponseDto>.SuccessResponse(user, "User retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ClerkId: {ClerkId}", clerkId);
                return StatusCode(500, ApiResponse<UserResponseDto>.ErrorResponse("An error occurred while retrieving the user"));
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
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
        /// Update user information
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<UserResponseDto>>> UpdateUser(Guid id, [FromBody] UpdateUserRequestDto request)
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

                var user = await _userService.UpdateUserAsync(id, request);
                
                if (user == null)
                {
                    return NotFound(ApiResponse<UserResponseDto>.ErrorResponse("User not found"));
                }

                return Ok(ApiResponse<UserResponseDto>.SuccessResponse(user, "User updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
                return StatusCode(500, ApiResponse<UserResponseDto>.ErrorResponse("An error occurred while updating the user"));
            }
        }

        /// <summary>
        /// Delete user (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(Guid id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);
                
                if (!result)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
                }

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

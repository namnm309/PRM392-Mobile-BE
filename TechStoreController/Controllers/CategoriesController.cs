using BAL.DTOs.Category;
using BAL.DTOs.Common;
using BAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechStoreController.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryResponseDto>>>> GetCategories([FromQuery] bool? isActive = null)
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync(isActive);
                return Ok(ApiResponse<IEnumerable<CategoryResponseDto>>.SuccessResponse(categories, "Categories retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, ApiResponse<IEnumerable<CategoryResponseDto>>.ErrorResponse("An error occurred while retrieving categories"));
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> GetCategory(Guid id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                    return NotFound(ApiResponse<CategoryResponseDto>.ErrorResponse("Category not found"));

                return Ok(ApiResponse<CategoryResponseDto>.SuccessResponse(category, "Category retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
                return StatusCode(500, ApiResponse<CategoryResponseDto>.ErrorResponse("An error occurred while retrieving the category"));
            }
        }

        [HttpPost]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> CreateCategory([FromBody] CreateCategoryRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<CategoryResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var category = await _categoryService.CreateCategoryAsync(request);
                return CreatedAtAction(
                    nameof(GetCategory),
                    new { id = category.Id },
                    ApiResponse<CategoryResponseDto>.SuccessResponse(category, "Category created successfully")
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<CategoryResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, ApiResponse<CategoryResponseDto>.ErrorResponse("An error occurred while creating the category"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<CategoryResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var category = await _categoryService.UpdateCategoryAsync(id, request);
                if (category == null)
                    return NotFound(ApiResponse<CategoryResponseDto>.ErrorResponse("Category not found"));

                return Ok(ApiResponse<CategoryResponseDto>.SuccessResponse(category, "Category updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<CategoryResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", id);
                return StatusCode(500, ApiResponse<CategoryResponseDto>.ErrorResponse("An error occurred while updating the category"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteCategory(Guid id)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);
                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResponse("Category not found"));

                return Ok(ApiResponse<object?>.SuccessResponse(null, "Category deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while deleting the category"));
            }
        }

        [HttpPost("{id}/toggle-active")]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> ToggleActive(Guid id)
        {
            try
            {
                var category = await _categoryService.ToggleActiveAsync(id);
                if (category == null)
                    return NotFound(ApiResponse<CategoryResponseDto>.ErrorResponse("Category not found"));

                return Ok(ApiResponse<CategoryResponseDto>.SuccessResponse(category, "Category status toggled successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling category status {CategoryId}", id);
                return StatusCode(500, ApiResponse<CategoryResponseDto>.ErrorResponse("An error occurred while toggling category status"));
            }
        }
    }
}

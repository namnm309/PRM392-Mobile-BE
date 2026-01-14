using BAL.DTOs.Brand;
using BAL.DTOs.Common;
using BAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechStoreController.Controllers
{
    /// <summary>
    /// Brands API Controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BrandsController : ControllerBase
    {
        private readonly IBrandService _brandService;
        private readonly ILogger<BrandsController> _logger;

        public BrandsController(IBrandService brandService, ILogger<BrandsController> logger)
        {
            _brandService = brandService;
            _logger = logger;
        }

        /// <summary>
        /// Get all brands (Public)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BrandResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<BrandResponseDto>>>> GetBrands([FromQuery] bool? isActive = null)
        {
            try
            {
                var brands = await _brandService.GetAllBrandsAsync(isActive);
                return Ok(ApiResponse<IEnumerable<BrandResponseDto>>.SuccessResponse(brands, "Brands retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving brands");
                return StatusCode(500, ApiResponse<IEnumerable<BrandResponseDto>>.ErrorResponse("An error occurred while retrieving brands"));
            }
        }

        /// <summary>
        /// Get brand by ID (Public)
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<BrandResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<BrandResponseDto>>> GetBrand(Guid id)
        {
            try
            {
                var brand = await _brandService.GetBrandByIdAsync(id);
                if (brand == null)
                    return NotFound(ApiResponse<BrandResponseDto>.ErrorResponse("Brand not found"));

                return Ok(ApiResponse<BrandResponseDto>.SuccessResponse(brand, "Brand retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving brand {BrandId}", id);
                return StatusCode(500, ApiResponse<BrandResponseDto>.ErrorResponse("An error occurred while retrieving the brand"));
            }
        }

        /// <summary>
        /// Create a new brand (Staff/Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<BrandResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<BrandResponseDto>>> CreateBrand([FromBody] CreateBrandRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<BrandResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var brand = await _brandService.CreateBrandAsync(request);
                return CreatedAtAction(
                    nameof(GetBrand),
                    new { id = brand.Id },
                    ApiResponse<BrandResponseDto>.SuccessResponse(brand, "Brand created successfully")
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<BrandResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating brand");
                return StatusCode(500, ApiResponse<BrandResponseDto>.ErrorResponse("An error occurred while creating the brand"));
            }
        }

        /// <summary>
        /// Update brand (Staff/Admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<BrandResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<BrandResponseDto>>> UpdateBrand(Guid id, [FromBody] UpdateBrandRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<BrandResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var brand = await _brandService.UpdateBrandAsync(id, request);
                if (brand == null)
                    return NotFound(ApiResponse<BrandResponseDto>.ErrorResponse("Brand not found"));

                return Ok(ApiResponse<BrandResponseDto>.SuccessResponse(brand, "Brand updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<BrandResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating brand {BrandId}", id);
                return StatusCode(500, ApiResponse<BrandResponseDto>.ErrorResponse("An error occurred while updating the brand"));
            }
        }

        /// <summary>
        /// Delete brand (Staff/Admin) - Soft delete, deactivates all related products
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteBrand(Guid id)
        {
            try
            {
                var result = await _brandService.DeleteBrandAsync(id);
                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResponse("Brand not found"));

                return Ok(ApiResponse<object?>.SuccessResponse(null, "Brand deleted successfully. All related products have been deactivated."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting brand {BrandId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while deleting the brand"));
            }
        }

        /// <summary>
        /// Toggle brand active status (Staff/Admin) - Deactivates all related products when brand is deactivated
        /// </summary>
        [HttpPost("{id}/toggle-active")]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<BrandResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<BrandResponseDto>>> ToggleActive(Guid id)
        {
            try
            {
                var brand = await _brandService.ToggleActiveAsync(id);
                if (brand == null)
                    return NotFound(ApiResponse<BrandResponseDto>.ErrorResponse("Brand not found"));

                return Ok(ApiResponse<BrandResponseDto>.SuccessResponse(brand, "Brand status toggled successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling brand status {BrandId}", id);
                return StatusCode(500, ApiResponse<BrandResponseDto>.ErrorResponse("An error occurred while toggling brand status"));
            }
        }
    }
}

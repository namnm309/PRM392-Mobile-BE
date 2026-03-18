using BAL.DTOs.Common;
using BAL.DTOs.Product;
using BAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechStoreController.Controllers
{
    [ApiController]
    [Route("api")]
    [Produces("application/json")]
    [Authorize(Policy = "AdminOnly")]
    public class ProductVariantsController : ControllerBase
    {
        private readonly IProductVariantService _variantService;
        private readonly ILogger<ProductVariantsController> _logger;

        public ProductVariantsController(IProductVariantService variantService, ILogger<ProductVariantsController> logger)
        {
            _variantService = variantService;
            _logger = logger;
        }

        // GET api/products/{productId}/variants?isActive=true
        [HttpGet("products/{productId}/variants")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductVariantResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductVariantResponseDto>>>> GetByProduct(Guid productId, [FromQuery] bool? isActive = null)
        {
            try
            {
                var items = await _variantService.GetByProductIdAsync(productId, isActive);
                return Ok(ApiResponse<IEnumerable<ProductVariantResponseDto>>.SuccessResponse(items, "Variants retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving variants for product {ProductId}", productId);
                return StatusCode(500, ApiResponse<IEnumerable<ProductVariantResponseDto>>.ErrorResponse("An error occurred while retrieving variants"));
            }
        }

        // GET api/product-variants/{id}
        [HttpGet("product-variants/{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ProductVariantResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProductVariantResponseDto>>> GetById(Guid id)
        {
            try
            {
                var item = await _variantService.GetByIdAsync(id);
                if (item == null)
                    return NotFound(ApiResponse<ProductVariantResponseDto>.ErrorResponse("Variant not found"));

                return Ok(ApiResponse<ProductVariantResponseDto>.SuccessResponse(item, "Variant retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving variant {VariantId}", id);
                return StatusCode(500, ApiResponse<ProductVariantResponseDto>.ErrorResponse("An error occurred while retrieving the variant"));
            }
        }

        // POST api/products/{productId}/variants
        [HttpPost("products/{productId}/variants")]
        [ProducesResponseType(typeof(ApiResponse<ProductVariantResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<ProductVariantResponseDto>>> Create(Guid productId, [FromBody] CreateProductVariantRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<ProductVariantResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var created = await _variantService.CreateAsync(productId, request);
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = created.Id },
                    ApiResponse<ProductVariantResponseDto>.SuccessResponse(created, "Variant created successfully")
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<ProductVariantResponseDto>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<ProductVariantResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating variant for product {ProductId}", productId);
                return StatusCode(500, ApiResponse<ProductVariantResponseDto>.ErrorResponse("An error occurred while creating the variant"));
            }
        }

        // PUT api/product-variants/{id}
        [HttpPut("product-variants/{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductVariantResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<ProductVariantResponseDto>>> Update(Guid id, [FromBody] UpdateProductVariantRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<ProductVariantResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var updated = await _variantService.UpdateAsync(id, request);
                if (updated == null)
                    return NotFound(ApiResponse<ProductVariantResponseDto>.ErrorResponse("Variant not found"));

                return Ok(ApiResponse<ProductVariantResponseDto>.SuccessResponse(updated, "Variant updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<ProductVariantResponseDto>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<ProductVariantResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating variant {VariantId}", id);
                return StatusCode(500, ApiResponse<ProductVariantResponseDto>.ErrorResponse("An error occurred while updating the variant"));
            }
        }

        // DELETE api/product-variants/{id}
        [HttpDelete("product-variants/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object?>>> Delete(Guid id)
        {
            try
            {
                var ok = await _variantService.DeleteAsync(id);
                if (!ok)
                    return NotFound(ApiResponse<object>.ErrorResponse("Variant not found"));

                return Ok(ApiResponse<object?>.SuccessResponse(null, "Variant deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting variant {VariantId}", id);
                return StatusCode(500, ApiResponse<object?>.ErrorResponse("An error occurred while deleting the variant"));
            }
        }
    }
}


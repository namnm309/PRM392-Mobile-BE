using BAL.DTOs.Common;
using BAL.DTOs.ProductImage;
using BAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechStoreController.Controllers
{
    /// <summary>
    /// ProductImages API Controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductImagesController : ControllerBase
    {
        private readonly IProductImageService _productImageService;
        private readonly ILogger<ProductImagesController> _logger;

        public ProductImagesController(IProductImageService productImageService, ILogger<ProductImagesController> logger)
        {
            _productImageService = productImageService;
            _logger = logger;
        }

        /// <summary>
        /// Get all images for a product (Public)
        /// </summary>
        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductImageResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductImageResponseDto>>>> GetProductImages(Guid productId)
        {
            try
            {
                var images = await _productImageService.GetProductImagesAsync(productId);
                return Ok(ApiResponse<IEnumerable<ProductImageResponseDto>>.SuccessResponse(images, "Product images retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product images for product {ProductId}", productId);
                return StatusCode(500, ApiResponse<IEnumerable<ProductImageResponseDto>>.ErrorResponse("An error occurred while retrieving product images"));
            }
        }

        /// <summary>
        /// Get product image by ID (Public)
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ProductImageResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProductImageResponseDto>>> GetProductImage(Guid id)
        {
            try
            {
                var image = await _productImageService.GetProductImageByIdAsync(id);
                if (image == null)
                    return NotFound(ApiResponse<ProductImageResponseDto>.ErrorResponse("Product image not found"));

                return Ok(ApiResponse<ProductImageResponseDto>.SuccessResponse(image, "Product image retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product image {ImageId}", id);
                return StatusCode(500, ApiResponse<ProductImageResponseDto>.ErrorResponse("An error occurred while retrieving the product image"));
            }
        }

        /// <summary>
        /// Create a new product image (Staff/Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<ProductImageResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<ProductImageResponseDto>>> CreateProductImage([FromBody] CreateProductImageRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<ProductImageResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var image = await _productImageService.CreateProductImageAsync(request);
                return CreatedAtAction(
                    nameof(GetProductImage),
                    new { id = image.Id },
                    ApiResponse<ProductImageResponseDto>.SuccessResponse(image, "Product image created successfully")
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<ProductImageResponseDto>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<ProductImageResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product image");
                return StatusCode(500, ApiResponse<ProductImageResponseDto>.ErrorResponse("An error occurred while creating the product image"));
            }
        }

        /// <summary>
        /// Update product image (Staff/Admin)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<ProductImageResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProductImageResponseDto>>> UpdateProductImage(Guid id, [FromBody] UpdateProductImageRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<ProductImageResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var image = await _productImageService.UpdateProductImageAsync(id, request);
                if (image == null)
                    return NotFound(ApiResponse<ProductImageResponseDto>.ErrorResponse("Product image not found"));

                return Ok(ApiResponse<ProductImageResponseDto>.SuccessResponse(image, "Product image updated successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<ProductImageResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product image {ImageId}", id);
                return StatusCode(500, ApiResponse<ProductImageResponseDto>.ErrorResponse("An error occurred while updating the product image"));
            }
        }

        /// <summary>
        /// Delete product image (Staff/Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteProductImage(Guid id)
        {
            try
            {
                var result = await _productImageService.DeleteProductImageAsync(id);
                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResponse("Product image not found"));

                return Ok(ApiResponse<object?>.SuccessResponse(null, "Product image deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product image {ImageId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while deleting the product image"));
            }
        }

        /// <summary>
        /// Set product image as main image (Staff/Admin)
        /// </summary>
        [HttpPost("{id}/set-main")]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<ProductImageResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<ProductImageResponseDto>>> SetMainImage(Guid id, [FromQuery] Guid productId)
        {
            try
            {
                var image = await _productImageService.SetMainImageAsync(productId, id);
                return Ok(ApiResponse<ProductImageResponseDto>.SuccessResponse(image, "Main image set successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<ProductImageResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting main image {ImageId} for product {ProductId}", id, productId);
                return StatusCode(500, ApiResponse<ProductImageResponseDto>.ErrorResponse("An error occurred while setting main image"));
            }
        }
    }
}

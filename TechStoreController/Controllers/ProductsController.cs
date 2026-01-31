using BAL.DTOs.Common;
using BAL.DTOs.Product;
using BAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechStoreController.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductResponseDto>>>> GetProducts(
            [FromQuery] bool? isActive = null,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] Guid? brandId = null)
        {
            try
            {
                var products = await _productService.GetAllProductsAsync(isActive, categoryId, brandId);
                return Ok(ApiResponse<IEnumerable<ProductResponseDto>>.SuccessResponse(products, "Products retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return StatusCode(500, ApiResponse<IEnumerable<ProductResponseDto>>.ErrorResponse("An error occurred while retrieving products"));
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ProductResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProductResponseDto>>> GetProduct(Guid id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                    return NotFound(ApiResponse<ProductResponseDto>.ErrorResponse("Product not found"));

                return Ok(ApiResponse<ProductResponseDto>.SuccessResponse(product, "Product retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product {ProductId}", id);
                return StatusCode(500, ApiResponse<ProductResponseDto>.ErrorResponse("An error occurred while retrieving the product"));
            }
        }

        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductResponseDto>>>> SearchProducts(
            [FromQuery] string? name = null,
            [FromQuery] Guid? brandId = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var products = await _productService.SearchProductsAsync(name, brandId, isActive);
                return Ok(ApiResponse<IEnumerable<ProductResponseDto>>.SuccessResponse(products, "Products retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products");
                return StatusCode(500, ApiResponse<IEnumerable<ProductResponseDto>>.ErrorResponse("An error occurred while searching products"));
            }
        }

        [HttpGet("{id}/compare")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ProductComparisonResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProductComparisonResponseDto>>> CompareProducts(Guid id)
        {
            try
            {
                var comparison = await _productService.CompareSimilarProductsAsync(id);
                return Ok(ApiResponse<ProductComparisonResponseDto>.SuccessResponse(comparison, "Product comparison retrieved successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<ProductComparisonResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing products for product {ProductId}", id);
                return StatusCode(500, ApiResponse<ProductComparisonResponseDto>.ErrorResponse("An error occurred while comparing products"));
            }
        }

        [HttpPost]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<ProductResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<ProductResponseDto>>> CreateProduct([FromBody] CreateProductRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<ProductResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var product = await _productService.CreateProductAsync(request);
                return CreatedAtAction(
                    nameof(GetProduct),
                    new { id = product.Id },
                    ApiResponse<ProductResponseDto>.SuccessResponse(product, "Product created successfully")
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<ProductResponseDto>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<ProductResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, ApiResponse<ProductResponseDto>.ErrorResponse("An error occurred while creating the product"));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<ProductResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProductResponseDto>>> UpdateProduct(Guid id, [FromBody] UpdateProductRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<ProductResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var product = await _productService.UpdateProductAsync(id, request);
                if (product == null)
                    return NotFound(ApiResponse<ProductResponseDto>.ErrorResponse("Product not found"));

                return Ok(ApiResponse<ProductResponseDto>.SuccessResponse(product, "Product updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<ProductResponseDto>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<ProductResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return StatusCode(500, ApiResponse<ProductResponseDto>.ErrorResponse("An error occurred while updating the product"));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteProduct(Guid id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResponse("Product not found"));

                return Ok(ApiResponse<object?>.SuccessResponse(null, "Product deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while deleting the product"));
            }
        }

        [HttpPost("{id}/toggle-active")]
        [Authorize(Policy = "StaffOrAdmin")]
        [ProducesResponseType(typeof(ApiResponse<ProductResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ProductResponseDto>>> ToggleActive(Guid id)
        {
            try
            {
                var product = await _productService.ToggleActiveAsync(id);
                if (product == null)
                    return NotFound(ApiResponse<ProductResponseDto>.ErrorResponse("Product not found"));

                return Ok(ApiResponse<ProductResponseDto>.SuccessResponse(product, "Product status toggled successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling product status {ProductId}", id);
                return StatusCode(500, ApiResponse<ProductResponseDto>.ErrorResponse("An error occurred while toggling product status"));
            }
        }
    }
}

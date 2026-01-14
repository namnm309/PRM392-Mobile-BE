using BAL.DTOs.Product;

namespace BAL.Services
{
    /// <summary>
    /// Product service interface
    /// </summary>
    public interface IProductService
    {
        Task<ProductResponseDto?> GetProductByIdAsync(Guid id);
        Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(bool? isActive = null, Guid? categoryId = null, Guid? brandId = null);
        Task<IEnumerable<ProductResponseDto>> SearchProductsAsync(string? name = null, Guid? brandId = null, bool? isActive = null);
        Task<ProductResponseDto> CreateProductAsync(CreateProductRequestDto request);
        Task<ProductResponseDto?> UpdateProductAsync(Guid id, UpdateProductRequestDto request);
        Task<bool> DeleteProductAsync(Guid id);
        Task<ProductResponseDto?> ToggleActiveAsync(Guid id);
        Task<ProductComparisonResponseDto> CompareSimilarProductsAsync(Guid productId);
    }
}

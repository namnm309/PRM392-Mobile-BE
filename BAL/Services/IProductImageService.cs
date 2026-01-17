using BAL.DTOs.ProductImage;

namespace BAL.Services
{
    /// <summary>
    /// ProductImage service interface
    /// </summary>
    public interface IProductImageService
    {
        Task<IEnumerable<ProductImageResponseDto>> GetProductImagesAsync(Guid productId);
        Task<ProductImageResponseDto?> GetProductImageByIdAsync(Guid id);
        Task<ProductImageResponseDto> CreateProductImageAsync(CreateProductImageRequestDto request);
        Task<ProductImageResponseDto?> UpdateProductImageAsync(Guid id, UpdateProductImageRequestDto request);
        Task<bool> DeleteProductImageAsync(Guid id);
        Task<ProductImageResponseDto> SetMainImageAsync(Guid productId, Guid imageId);
    }
}

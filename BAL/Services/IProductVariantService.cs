using BAL.DTOs.Product;

namespace BAL.Services
{
    public interface IProductVariantService
    {
        Task<ProductVariantResponseDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<ProductVariantResponseDto>> GetByProductIdAsync(Guid productId, bool? isActive = null);
        Task<ProductVariantResponseDto> CreateAsync(Guid productId, CreateProductVariantRequestDto request);
        Task<ProductVariantResponseDto?> UpdateAsync(Guid id, UpdateProductVariantRequestDto request);
        Task<bool> DeleteAsync(Guid id);
    }
}


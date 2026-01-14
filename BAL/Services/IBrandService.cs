using BAL.DTOs.Brand;

namespace BAL.Services
{
    /// <summary>
    /// Brand service interface
    /// </summary>
    public interface IBrandService
    {
        Task<BrandResponseDto?> GetBrandByIdAsync(Guid id);
        Task<IEnumerable<BrandResponseDto>> GetAllBrandsAsync(bool? isActive = null);
        Task<BrandResponseDto> CreateBrandAsync(CreateBrandRequestDto request);
        Task<BrandResponseDto?> UpdateBrandAsync(Guid id, UpdateBrandRequestDto request);
        Task<bool> DeleteBrandAsync(Guid id);
        Task<BrandResponseDto?> ToggleActiveAsync(Guid id);
    }
}

using BAL.DTOs.Category;

namespace BAL.Services
{
    /// <summary>
    /// Category service interface
    /// </summary>
    public interface ICategoryService
    {
        Task<CategoryResponseDto?> GetCategoryByIdAsync(Guid id);
        Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync(bool? isActive = null);
        Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryRequestDto request);
        Task<IEnumerable<CategoryResponseDto>> BulkCreateCategoriesAsync(IEnumerable<BulkCreateCategoryItemDto> items);
        Task<CategoryResponseDto?> UpdateCategoryAsync(Guid id, UpdateCategoryRequestDto request);
        Task<bool> DeleteCategoryAsync(Guid id);
        Task<CategoryResponseDto?> ToggleActiveAsync(Guid id);
    }
}

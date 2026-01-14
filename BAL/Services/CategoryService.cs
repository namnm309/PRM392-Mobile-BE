using BAL.DTOs.Category;
using DAL.Models;
using DAL.Repositories;

namespace BAL.Services
{
    /// <summary>
    /// Category service implementation
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<CategoryResponseDto?> GetCategoryByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            return category == null ? null : MapToDto(category);
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync(bool? isActive = null)
        {
            IEnumerable<Category> categories;
            
            if (isActive.HasValue && isActive.Value)
            {
                categories = await _categoryRepository.GetActiveCategoriesAsync();
            }
            else if (isActive.HasValue && !isActive.Value)
            {
                categories = await _categoryRepository.FindAsync(c => !c.IsActive);
            }
            else
            {
                categories = await _categoryRepository.GetAllAsync();
            }

            return categories.Select(MapToDto);
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryRequestDto request)
        {
            // Business rule: Check if name already exists
            var existing = await _categoryRepository.GetByNameAsync(request.Name);
            if (existing != null)
            {
                throw new InvalidOperationException($"Category with name '{request.Name}' already exists");
            }

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _categoryRepository.AddAsync(category);
            return MapToDto(created);
        }

        public async Task<CategoryResponseDto?> UpdateCategoryAsync(Guid id, UpdateCategoryRequestDto request)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return null;

            // Business rule: Check name uniqueness if name is being updated
            if (request.Name != null && request.Name != category.Name)
            {
                var existing = await _categoryRepository.GetByNameAsync(request.Name);
                if (existing != null && existing.Id != id)
                {
                    throw new InvalidOperationException($"Category with name '{request.Name}' already exists");
                }
                category.Name = request.Name;
            }

            if (request.Description != null)
                category.Description = request.Description;

            if (request.IsActive.HasValue)
                category.IsActive = request.IsActive.Value;

            category.UpdatedAt = DateTime.UtcNow;

            var updated = await _categoryRepository.UpdateAsync(category);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            // Business rule: Soft delete - set IsActive = false
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return false;

            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;
            await _categoryRepository.UpdateAsync(category);
            return true;
        }

        public async Task<CategoryResponseDto?> ToggleActiveAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return null;

            category.IsActive = !category.IsActive;
            category.UpdatedAt = DateTime.UtcNow;
            var updated = await _categoryRepository.UpdateAsync(category);
            return MapToDto(updated);
        }

        private static CategoryResponseDto MapToDto(Category category)
        {
            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }
    }
}

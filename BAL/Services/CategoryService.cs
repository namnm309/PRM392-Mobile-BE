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
        private readonly IProductRepository _productRepository;

        public CategoryService(ICategoryRepository categoryRepository, IProductRepository productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        public async Task<CategoryResponseDto?> GetCategoryByIdAsync(Guid id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return null;

            var productCount = await _productRepository.CountAsync(p => p.CategoryId == id);
            return MapToDto(category, productCount, []);
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync(bool? isActive = null)
        {
            var categories = await _categoryRepository.GetCategoriesWithChildrenAsync(isActive);
            var categoryList = categories.ToList();

            // Build tree: root categories (ParentId == null)
            var rootCategories = categoryList.Where(c => c.ParentId == null).ToList();
            var productCounts = await GetProductCountsAsync(categoryList.Select(c => c.Id));

            return rootCategories.Select(root => BuildCategoryTree(root, categoryList, productCounts));
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
                ParentId = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _categoryRepository.AddAsync(category);
            var childDtos = new List<CategoryResponseDto>();

            // Tạo các category con nếu có
            foreach (var childName in request.Children.Where(c => !string.IsNullOrWhiteSpace(c)))
            {
                var childExisting = await _categoryRepository.GetByNameAsync(childName.Trim());
                if (childExisting != null)
                    throw new InvalidOperationException($"Child category with name '{childName}' already exists");

                var child = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = childName.Trim(),
                    Description = null,
                    IsActive = true,
                    ParentId = created.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _categoryRepository.AddAsync(child);
                childDtos.Add(MapToDto(child, 0, []));
            }

            var productCount = await _productRepository.CountAsync(p => p.CategoryId == created.Id);
            return MapToDto(created, productCount, childDtos);
        }

        public async Task<IEnumerable<CategoryResponseDto>> BulkCreateCategoriesAsync(IEnumerable<BulkCreateCategoryItemDto> items)
        {
            var result = new List<CategoryResponseDto>();
            var itemsList = items.ToList();

            foreach (var item in itemsList)
            {
                // Tạo category cha
                var existing = await _categoryRepository.GetByNameAsync(item.Name);
                if (existing != null)
                    throw new InvalidOperationException($"Category with name '{item.Name}' already exists");

                var parent = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = item.Name,
                    Description = item.Description,
                    IsActive = true,
                    ParentId = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _categoryRepository.AddAsync(parent);

                var childDtos = new List<CategoryResponseDto>();

                // Tạo các category con
                foreach (var childName in item.Children.Where(c => !string.IsNullOrWhiteSpace(c)))
                {
                    var childExisting = await _categoryRepository.GetByNameAsync(childName.Trim());
                    if (childExisting != null)
                        throw new InvalidOperationException($"Child category with name '{childName}' already exists");

                    var child = new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = childName.Trim(),
                        Description = null,
                        IsActive = true,
                        ParentId = parent.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _categoryRepository.AddAsync(child);
                    childDtos.Add(MapToDto(child, 0, []));
                }

                var parentProductCount = await _productRepository.CountAsync(p => p.CategoryId == parent.Id);
                result.Add(MapToDto(parent, parentProductCount, childDtos));
            }

            return result;
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
            var productCount = await _productRepository.CountAsync(p => p.CategoryId == id);
            return MapToDto(updated, productCount, []);
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
            var productCount = await _productRepository.CountAsync(p => p.CategoryId == id);
            return MapToDto(updated, productCount, []);
        }

        private async Task<Dictionary<Guid, int>> GetProductCountsAsync(IEnumerable<Guid> categoryIds)
        {
            return await _productRepository.GetProductCountByCategoryIdsAsync(categoryIds);
        }

        private CategoryResponseDto BuildCategoryTree(
            Category category,
            List<Category> allCategories,
            Dictionary<Guid, int> productCounts)
        {
            var children = allCategories.Where(c => c.ParentId == category.Id).ToList();
            var productCount = productCounts.GetValueOrDefault(category.Id, 0);

            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ProductCount = productCount,
                Children = children.Select(c => BuildCategoryTree(c, allCategories, productCounts)).ToList(),
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }

        private static CategoryResponseDto MapToDto(Category category, int productCount, List<CategoryResponseDto> children)
        {
            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ProductCount = productCount,
                Children = children,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }
    }
}

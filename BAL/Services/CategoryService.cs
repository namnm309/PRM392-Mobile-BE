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

        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetCategoriesWithChildrenAsync();
            var categoryList = categories.ToList();
            var activeParentIds = categoryList.Select(c => c.Id).ToHashSet();

            // Root = ParentId == null OR parent is inactive (not in current list) - show orphaned children at root level
            var rootCategories = categoryList
                .Where(c => c.ParentId == null || !activeParentIds.Contains(c.ParentId.Value))
                .ToList();
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
                ImageUrl = request.ImageUrl,
                DisplayOrder = request.DisplayOrder,
                IsHot = request.IsHot,
                ParentId = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _categoryRepository.AddAsync(category);
            var childDtos = new List<CategoryResponseDto>();

            // Tạo các category con nếu có
            foreach (var childItem in request.Children.Where(c => !string.IsNullOrWhiteSpace(c.Name)))
            {
                var childExisting = await _categoryRepository.GetByNameAsync(childItem.Name.Trim());
                if (childExisting != null)
                    throw new InvalidOperationException($"Child category with name '{childItem.Name}' already exists");

                var child = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = childItem.Name.Trim(),
                    Description = null,
                    ImageUrl = childItem.ImageUrl,
                    DisplayOrder = childItem.DisplayOrder,
                    IsHot = childItem.IsHot,
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
                    ImageUrl = item.ImageUrl,
                    DisplayOrder = item.DisplayOrder,
                    IsHot = item.IsHot,
                    ParentId = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _categoryRepository.AddAsync(parent);

                var childDtos = new List<CategoryResponseDto>();

                // Tạo các category con
                foreach (var childItem in item.Children.Where(c => !string.IsNullOrWhiteSpace(c.Name)))
                {
                    var childExisting = await _categoryRepository.GetByNameAsync(childItem.Name.Trim());
                    if (childExisting != null)
                        throw new InvalidOperationException($"Child category with name '{childItem.Name}' already exists");

                    var child = new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = childItem.Name.Trim(),
                        Description = null,
                        ImageUrl = childItem.ImageUrl,
                        DisplayOrder = childItem.DisplayOrder,
                        IsHot = childItem.IsHot,
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

            if (request.ImageUrl != null)
                category.ImageUrl = request.ImageUrl;

            if (request.DisplayOrder.HasValue)
                category.DisplayOrder = request.DisplayOrder.Value;

            if (request.IsHot.HasValue)
                category.IsHot = request.IsHot.Value;

            if (request.ParentId.HasValue)
                category.ParentId = request.ParentId.Value;

            category.UpdatedAt = DateTime.UtcNow;

            var updated = await _categoryRepository.UpdateAsync(category);
            var productCount = await _productRepository.CountAsync(p => p.CategoryId == id);
            return MapToDto(updated, productCount, []);
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            // Hard delete: remove category from database
            return await _categoryRepository.DeleteAsync(id);
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
            var children = allCategories
                .Where(c => c.ParentId == category.Id)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToList();
            var productCount = productCounts.GetValueOrDefault(category.Id, 0);

            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                DisplayOrder = category.DisplayOrder,
                IsHot = category.IsHot,
                ProductCount = productCount,
                Children = children.Select(c => BuildCategoryTree(c, allCategories, productCounts)).ToList(),
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
                ImageUrl = category.ImageUrl,
                DisplayOrder = category.DisplayOrder,
                IsHot = category.IsHot,
                ProductCount = productCount,
                Children = children,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }
    }
}

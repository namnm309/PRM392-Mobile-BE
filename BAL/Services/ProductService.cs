using BAL.DTOs.Category;
using BAL.DTOs.Brand;
using BAL.DTOs.Product;
using BAL.DTOs.ProductImage;
using DAL.Models;
using DAL.Repositories;

namespace BAL.Services
{
    /// <summary>
    /// Product service implementation
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;

        public ProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IBrandRepository brandRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
        }

        public async Task<ProductResponseDto?> GetProductByIdAsync(Guid id)
        {
            var product = await _productRepository.GetByIdWithDetailsAsync(id);
            return product == null ? null : MapToDto(product);
        }

        public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(bool? isActive = null, Guid? categoryId = null, Guid? brandId = null)
        {
            var products = await _productRepository.GetAllWithFiltersAsync(isActive, categoryId, brandId);
            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductResponseDto>> SearchProductsAsync(string? name = null, Guid? brandId = null, bool? isActive = null)
        {
            IEnumerable<Product> products;

            if (!string.IsNullOrWhiteSpace(name) && brandId.HasValue)
            {
                // Search by both name and brand
                var byName = await _productRepository.SearchByNameAsync(name, isActive);
                products = byName.Where(p => p.BrandId == brandId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(name))
            {
                // Search by name only
                products = await _productRepository.SearchByNameAsync(name, isActive);
            }
            else if (brandId.HasValue)
            {
                // Search by brand only
                products = await _productRepository.SearchByBrandIdAsync(brandId.Value, isActive);
            }
            else
            {
                // Get all with filters
                products = await _productRepository.GetAllWithFiltersAsync(isActive);
            }

            return products.Select(MapToDto);
        }

        public async Task<ProductResponseDto> CreateProductAsync(CreateProductRequestDto request)
        {
            // Business rule: Validate Category exists if provided
            if (request.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
                if (category == null)
                {
                    throw new InvalidOperationException("Category not found");
                }
            }

            // Business rule: Validate Brand exists if provided
            if (request.BrandId.HasValue)
            {
                var brand = await _brandRepository.GetByIdAsync(request.BrandId.Value);
                if (brand == null)
                {
                    throw new InvalidOperationException("Brand not found");
                }
            }

            // Business rule: Validate DiscountPrice < Price
            if (request.DiscountPrice.HasValue && request.DiscountPrice.Value >= request.Price)
            {
                throw new ArgumentException("Discount price must be less than regular price");
            }

            // Business rule: Stock cannot be negative
            if (request.Stock < 0)
            {
                throw new ArgumentException("Stock cannot be negative");
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                DiscountPrice = request.DiscountPrice,
                Stock = request.Stock,
                ImageUrl = request.ImageUrl,
                CategoryId = request.CategoryId,
                BrandId = request.BrandId,
                IsActive = request.IsActive,
                IsOnSale = request.IsOnSale,
                NoVoucherTag = request.NoVoucherTag,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _productRepository.AddAsync(product);
            var productWithDetails = await _productRepository.GetByIdWithDetailsAsync(created.Id);
            return MapToDto(productWithDetails!);
        }

        public async Task<ProductResponseDto?> UpdateProductAsync(Guid id, UpdateProductRequestDto request)
        {
            var product = await _productRepository.GetByIdWithTrackingAsync(id);
            if (product == null)
                return null;

            // Business rule: Validate Category exists if being updated
            if (request.CategoryId.HasValue)
            {
                var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
                if (category == null)
                {
                    throw new InvalidOperationException("Category not found");
                }
                product.CategoryId = request.CategoryId.Value;
            }

            // Business rule: Validate Brand exists if being updated
            if (request.BrandId.HasValue)
            {
                var brand = await _brandRepository.GetByIdAsync(request.BrandId.Value);
                if (brand == null)
                {
                    throw new InvalidOperationException("Brand not found");
                }
                product.BrandId = request.BrandId.Value;
            }

            if (request.Name != null)
                product.Name = request.Name;

            if (request.Description != null)
                product.Description = request.Description;

            if (request.Price.HasValue)
            {
                product.Price = request.Price.Value;
                
                // Business rule: If DiscountPrice exists and is >= Price, invalidate it
                if (product.DiscountPrice.HasValue && product.DiscountPrice.Value >= product.Price)
                {
                    product.DiscountPrice = null;
                }
            }

            if (request.DiscountPrice.HasValue)
            {
                // Business rule: Validate DiscountPrice < Price
                var price = request.Price ?? product.Price;
                if (request.DiscountPrice.Value >= price)
                {
                    throw new ArgumentException("Discount price must be less than regular price");
                }
                product.DiscountPrice = request.DiscountPrice.Value;
            }

            if (request.Stock.HasValue)
            {
                // Business rule: Stock cannot be negative
                if (request.Stock.Value < 0)
                {
                    throw new ArgumentException("Stock cannot be negative");
                }
                product.Stock = request.Stock.Value;
            }

            if (request.ImageUrl != null)
                product.ImageUrl = request.ImageUrl;

            if (request.IsActive.HasValue)
                product.IsActive = request.IsActive.Value;

            if (request.IsOnSale.HasValue)
                product.IsOnSale = request.IsOnSale.Value;

            if (request.NoVoucherTag.HasValue)
                product.NoVoucherTag = request.NoVoucherTag.Value;

            product.UpdatedAt = DateTime.UtcNow;
            var updated = await _productRepository.UpdateAsync(product);
            var productWithDetails = await _productRepository.GetByIdWithDetailsAsync(updated.Id);
            return MapToDto(productWithDetails!);
        }

        public async Task<bool> DeleteProductAsync(Guid id)
        {
            // Business rule: Soft delete - set IsActive = false
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return false;

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(product);
            return true;
        }

        public async Task<ProductResponseDto?> ToggleActiveAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return null;

            product.IsActive = !product.IsActive;
            product.UpdatedAt = DateTime.UtcNow;
            var updated = await _productRepository.UpdateAsync(product);
            var productWithDetails = await _productRepository.GetByIdWithDetailsAsync(updated.Id);
            return MapToDto(productWithDetails!);
        }

        public async Task<ProductComparisonResponseDto> CompareSimilarProductsAsync(Guid productId)
        {
            var originalProduct = await _productRepository.GetByIdWithDetailsAsync(productId);
            if (originalProduct == null)
            {
                throw new InvalidOperationException("Product not found");
            }

            var similarProducts = await _productRepository.GetSimilarProductsAsync(productId, 5);
            
            return new ProductComparisonResponseDto
            {
                OriginalProduct = MapToDto(originalProduct),
                SimilarProducts = similarProducts.Select(MapToDto).ToList()
            };
        }

        private static ProductResponseDto MapToDto(Product product)
        {
            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                Category = product.Category != null ? new CategoryResponseDto
                {
                    Id = product.Category.Id,
                    Name = product.Category.Name,
                    Description = product.Category.Description,
                    ProductCount = 0,
                    Children = [],
                    IsActive = product.Category.IsActive,
                    CreatedAt = product.Category.CreatedAt,
                    UpdatedAt = product.Category.UpdatedAt
                } : null,
                BrandId = product.BrandId,
                Brand = product.Brand != null ? new BrandResponseDto
                {
                    Id = product.Brand.Id,
                    Name = product.Brand.Name,
                    Description = product.Brand.Description,
                    IsActive = product.Brand.IsActive,
                    CreatedAt = product.Brand.CreatedAt,
                    UpdatedAt = product.Brand.UpdatedAt
                } : null,
                IsActive = product.IsActive,
                IsOnSale = product.IsOnSale,
                NoVoucherTag = product.NoVoucherTag,
                ProductImages = product.ProductImages?.Select(pi => new ProductImageResponseDto
                {
                    Id = pi.Id,
                    ProductId = pi.ProductId,
                    ImageUrl = pi.ImageUrl,
                    ImageType = pi.ImageType,
                    DisplayOrder = pi.DisplayOrder,
                    CreatedAt = pi.CreatedAt,
                    UpdatedAt = pi.UpdatedAt
                }).ToList() ?? new List<ProductImageResponseDto>(),
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}

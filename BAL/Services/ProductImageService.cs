using BAL.DTOs.ProductImage;
using DAL.Data;
using DAL.Models;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BAL.Services
{
    /// <summary>
    /// ProductImage service implementation
    /// </summary>
    public class ProductImageService : IProductImageService
    {
        private readonly IProductImageRepository _productImageRepository;
        private readonly IProductRepository _productRepository;
        private readonly TechStoreContext _context;

        public ProductImageService(
            IProductImageRepository productImageRepository,
            IProductRepository productRepository,
            TechStoreContext context)
        {
            _productImageRepository = productImageRepository;
            _productRepository = productRepository;
            _context = context;
        }

        public async Task<IEnumerable<ProductImageResponseDto>> GetProductImagesAsync(Guid productId)
        {
            var images = await _productImageRepository.GetByProductIdAsync(productId);
            return images.Select(MapToDto);
        }

        public async Task<ProductImageResponseDto?> GetProductImageByIdAsync(Guid id)
        {
            var image = await _productImageRepository.GetByIdAsync(id);
            return image == null ? null : MapToDto(image);
        }

        public async Task<ProductImageResponseDto> CreateProductImageAsync(CreateProductImageRequestDto request)
        {
            // Business rule: Validate product exists
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found");
            }

            // Business rule: Validate ImageType
            var validTypes = new[] { "Main", "Sub", "Poster", "Thumbnail" };
            if (!validTypes.Contains(request.ImageType))
            {
                throw new ArgumentException($"Invalid ImageType. Must be one of: {string.Join(", ", validTypes)}");
            }

            // Business rule: If setting as Main, unset other Main images
            if (request.ImageType == "Main")
            {
                var existingMain = await _productImageRepository.GetMainImageByProductIdAsync(request.ProductId);
                if (existingMain != null)
                {
                    existingMain.ImageType = "Sub";
                    existingMain.UpdatedAt = DateTime.UtcNow;
                    await _productImageRepository.UpdateAsync(existingMain);
                }
            }

            var image = new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                ImageUrl = request.ImageUrl,
                ImageType = request.ImageType,
                DisplayOrder = request.DisplayOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _productImageRepository.AddAsync(image);
            return MapToDto(created);
        }

        public async Task<ProductImageResponseDto?> UpdateProductImageAsync(Guid id, UpdateProductImageRequestDto request)
        {
            var image = await _productImageRepository.GetByIdAsync(id);
            if (image == null)
                return null;

            // Business rule: Validate ImageType if being updated
            if (request.ImageType != null)
            {
                var validTypes = new[] { "Main", "Sub", "Poster", "Thumbnail" };
                if (!validTypes.Contains(request.ImageType))
                {
                    throw new ArgumentException($"Invalid ImageType. Must be one of: {string.Join(", ", validTypes)}");
                }

                // Business rule: If setting as Main, unset other Main images
                if (request.ImageType == "Main" && image.ImageType != "Main")
                {
                    var existingMain = await _productImageRepository.GetMainImageByProductIdAsync(image.ProductId);
                    if (existingMain != null && existingMain.Id != id)
                    {
                        existingMain.ImageType = "Sub";
                        existingMain.UpdatedAt = DateTime.UtcNow;
                        await _productImageRepository.UpdateAsync(existingMain);
                    }
                }

                image.ImageType = request.ImageType;
            }

            if (request.ImageUrl != null)
                image.ImageUrl = request.ImageUrl;

            if (request.DisplayOrder.HasValue)
                image.DisplayOrder = request.DisplayOrder.Value;

            image.UpdatedAt = DateTime.UtcNow;
            var updated = await _productImageRepository.UpdateAsync(image);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteProductImageAsync(Guid id)
        {
            return await _productImageRepository.DeleteAsync(id);
        }

        public async Task<ProductImageResponseDto> SetMainImageAsync(Guid productId, Guid imageId)
        {
            // Business rule: Verify image belongs to product
            var image = await _productImageRepository.GetByIdAsync(imageId);
            if (image == null || image.ProductId != productId)
            {
                throw new InvalidOperationException("Image not found or does not belong to product");
            }

            // Business rule: Unset other Main images
            var existingMain = await _productImageRepository.GetMainImageByProductIdAsync(productId);
            if (existingMain != null && existingMain.Id != imageId)
            {
                existingMain.ImageType = "Sub";
                existingMain.UpdatedAt = DateTime.UtcNow;
                await _productImageRepository.UpdateAsync(existingMain);
            }

            image.ImageType = "Main";
            image.UpdatedAt = DateTime.UtcNow;
            var updated = await _productImageRepository.UpdateAsync(image);
            return MapToDto(updated);
        }

        private static ProductImageResponseDto MapToDto(ProductImage image)
        {
            return new ProductImageResponseDto
            {
                Id = image.Id,
                ProductId = image.ProductId,
                ImageUrl = image.ImageUrl,
                ImageType = image.ImageType,
                DisplayOrder = image.DisplayOrder,
                CreatedAt = image.CreatedAt,
                UpdatedAt = image.UpdatedAt
            };
        }
    }
}

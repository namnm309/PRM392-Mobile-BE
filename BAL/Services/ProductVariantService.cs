using BAL.DTOs.Product;
using DAL.Models;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BAL.Services
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly IProductVariantRepository _variantRepository;
        private readonly IProductRepository _productRepository;

        public ProductVariantService(IProductVariantRepository variantRepository, IProductRepository productRepository)
        {
            _variantRepository = variantRepository;
            _productRepository = productRepository;
        }

        public async Task<ProductVariantResponseDto?> GetByIdAsync(Guid id)
        {
            var v = await _variantRepository.GetByIdAsync(id);
            return v == null ? null : MapToDto(v);
        }

        public async Task<IEnumerable<ProductVariantResponseDto>> GetByProductIdAsync(Guid productId, bool? isActive = null)
        {
            var items = await _variantRepository.GetByProductIdAsync(productId, isActive);
            return items.Select(MapToDto);
        }

        public async Task<ProductVariantResponseDto> CreateAsync(Guid productId, CreateProductVariantRequestDto request)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new InvalidOperationException("Product not found");

            if (request.DiscountPrice.HasValue && request.DiscountPrice.Value >= request.Price)
                throw new ArgumentException("Discount price must be less than regular price");

            var now = DateTime.UtcNow;
            var entity = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Sku = string.IsNullOrWhiteSpace(request.Sku) ? null : request.Sku.Trim(),
                VariantName = string.IsNullOrWhiteSpace(request.VariantName) ? null : request.VariantName.Trim(),
                ColorName = request.ColorName.Trim(),
                ColorHex = string.IsNullOrWhiteSpace(request.ColorHex) ? null : request.ColorHex.Trim(),
                RamGb = request.RamGb,
                StorageGb = request.StorageGb,
                ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
                Price = request.Price,
                DiscountPrice = request.DiscountPrice,
                Stock = request.Stock,
                IsActive = request.IsActive,
                DisplayOrder = request.DisplayOrder,
                CreatedAt = now,
                UpdatedAt = now
            };

            try
            {
                var created = await _variantRepository.AddAsync(entity);
                return MapToDto(created);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Variant already exists or violates constraints: {ex.Message}");
            }
        }

        public async Task<ProductVariantResponseDto?> UpdateAsync(Guid id, UpdateProductVariantRequestDto request)
        {
            var entity = await _variantRepository.GetByIdAsync(id);
            if (entity == null)
                return null;

            if (request.Sku != null)
                entity.Sku = string.IsNullOrWhiteSpace(request.Sku) ? null : request.Sku.Trim();
            if (request.VariantName != null)
                entity.VariantName = string.IsNullOrWhiteSpace(request.VariantName) ? null : request.VariantName.Trim();
            if (request.ColorName != null)
                entity.ColorName = request.ColorName.Trim();
            if (request.ColorHex != null)
                entity.ColorHex = string.IsNullOrWhiteSpace(request.ColorHex) ? null : request.ColorHex.Trim();
            if (request.RamGb.HasValue)
                entity.RamGb = request.RamGb.Value;
            if (request.StorageGb.HasValue)
                entity.StorageGb = request.StorageGb.Value;
            if (request.ImageUrl != null)
                entity.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
            if (request.Price.HasValue)
                entity.Price = request.Price.Value;
            if (request.DiscountPrice.HasValue)
                entity.DiscountPrice = request.DiscountPrice.Value;
            if (request.Stock.HasValue)
                entity.Stock = request.Stock.Value;
            if (request.IsActive.HasValue)
                entity.IsActive = request.IsActive.Value;
            if (request.DisplayOrder.HasValue)
                entity.DisplayOrder = request.DisplayOrder.Value;

            if (entity.DiscountPrice.HasValue && entity.DiscountPrice.Value >= entity.Price)
                throw new ArgumentException("Discount price must be less than regular price");

            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                var updated = await _variantRepository.UpdateAsync(entity);
                return MapToDto(updated);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Variant update violates constraints: {ex.Message}");
            }
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            return _variantRepository.DeleteAsync(id);
        }

        private static ProductVariantResponseDto MapToDto(ProductVariant v)
        {
            return new ProductVariantResponseDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                Sku = v.Sku,
                VariantName = v.VariantName,
                ColorName = v.ColorName,
                ColorHex = v.ColorHex,
                RamGb = v.RamGb,
                StorageGb = v.StorageGb,
                ImageUrl = v.ImageUrl,
                Price = v.Price,
                DiscountPrice = v.DiscountPrice,
                Stock = v.Stock,
                IsActive = v.IsActive,
                DisplayOrder = v.DisplayOrder,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt
            };
        }
    }
}


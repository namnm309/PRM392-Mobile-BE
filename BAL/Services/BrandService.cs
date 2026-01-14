using BAL.DTOs.Brand;
using DAL.Data;
using DAL.Models;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BAL.Services
{
    /// <summary>
    /// Brand service implementation
    /// </summary>
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly TechStoreContext _context;

        public BrandService(IBrandRepository brandRepository, TechStoreContext context)
        {
            _brandRepository = brandRepository;
            _context = context;
        }

        public async Task<BrandResponseDto?> GetBrandByIdAsync(Guid id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            return brand == null ? null : MapToDto(brand);
        }

        public async Task<IEnumerable<BrandResponseDto>> GetAllBrandsAsync(bool? isActive = null)
        {
            IEnumerable<Brand> brands;
            
            if (isActive.HasValue && isActive.Value)
            {
                brands = await _brandRepository.GetActiveBrandsAsync();
            }
            else if (isActive.HasValue && !isActive.Value)
            {
                brands = await _brandRepository.FindAsync(b => !b.IsActive);
            }
            else
            {
                brands = await _brandRepository.GetAllAsync();
            }

            return brands.Select(MapToDto);
        }

        public async Task<BrandResponseDto> CreateBrandAsync(CreateBrandRequestDto request)
        {
            // Business rule: Check if name already exists
            var existing = await _brandRepository.GetByNameAsync(request.Name);
            if (existing != null)
            {
                throw new InvalidOperationException($"Brand with name '{request.Name}' already exists");
            }

            var brand = new Brand
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _brandRepository.AddAsync(brand);
            return MapToDto(created);
        }

        public async Task<BrandResponseDto?> UpdateBrandAsync(Guid id, UpdateBrandRequestDto request)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
                return null;

            // Business rule: Check name uniqueness if name is being updated
            if (request.Name != null && request.Name != brand.Name)
            {
                var existing = await _brandRepository.GetByNameAsync(request.Name);
                if (existing != null && existing.Id != id)
                {
                    throw new InvalidOperationException($"Brand with name '{request.Name}' already exists");
                }
                brand.Name = request.Name;
            }

            if (request.Description != null)
                brand.Description = request.Description;

            if (request.IsActive.HasValue)
            {
                brand.IsActive = request.IsActive.Value;
                
                // Business rule: When brand is deactivated, deactivate all related products
                if (!request.IsActive.Value)
                {
                    var products = await _context.Products
                        .Where(p => p.BrandId == id && p.IsActive)
                        .ToListAsync();
                    
                    foreach (var product in products)
                    {
                        product.IsActive = false;
                        product.UpdatedAt = DateTime.UtcNow;
                    }
                    
                    if (products.Any())
                    {
                        await _context.SaveChangesAsync();
                    }
                }
            }

            brand.UpdatedAt = DateTime.UtcNow;
            var updated = await _brandRepository.UpdateAsync(brand);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteBrandAsync(Guid id)
        {
            // Business rule: Soft delete - set IsActive = false and deactivate products
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
                return false;

            brand.IsActive = false;
            brand.UpdatedAt = DateTime.UtcNow;
            await _brandRepository.UpdateAsync(brand);

            // Deactivate all related products
            var products = await _context.Products
                .Where(p => p.BrandId == id && p.IsActive)
                .ToListAsync();
            
            foreach (var product in products)
            {
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;
            }
            
            if (products.Any())
            {
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<BrandResponseDto?> ToggleActiveAsync(Guid id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
                return null;

            brand.IsActive = !brand.IsActive;
            brand.UpdatedAt = DateTime.UtcNow;

            // Business rule: When brand is deactivated, deactivate all related products
            if (!brand.IsActive)
            {
                var products = await _context.Products
                    .Where(p => p.BrandId == id && p.IsActive)
                    .ToListAsync();
                
                foreach (var product in products)
                {
                    product.IsActive = false;
                    product.UpdatedAt = DateTime.UtcNow;
                }
                
                if (products.Any())
                {
                    await _context.SaveChangesAsync();
                }
            }

            var updated = await _brandRepository.UpdateAsync(brand);
            return MapToDto(updated);
        }

        private static BrandResponseDto MapToDto(Brand brand)
        {
            return new BrandResponseDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                IsActive = brand.IsActive,
                CreatedAt = brand.CreatedAt,
                UpdatedAt = brand.UpdatedAt
            };
        }
    }
}

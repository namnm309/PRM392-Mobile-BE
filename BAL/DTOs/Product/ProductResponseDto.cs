using BAL.DTOs.Category;
using BAL.DTOs.Brand;
using BAL.DTOs.ProductImage;

namespace BAL.DTOs.Product
{
    /// <summary>
    /// Product response DTO with full details
    /// </summary>
    public class ProductResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public Guid? CategoryId { get; set; }
        public CategoryResponseDto? Category { get; set; }
        public Guid? BrandId { get; set; }
        public BrandResponseDto? Brand { get; set; }
        public bool IsActive { get; set; }
        public bool IsOnSale { get; set; }
        public bool NoVoucherTag { get; set; }
        public List<ProductImageResponseDto> ProductImages { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

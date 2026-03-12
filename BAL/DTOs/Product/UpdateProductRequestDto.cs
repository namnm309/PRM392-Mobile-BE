using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Product
{
    /// <summary>
    /// Update product request DTO
    /// </summary>
    public class UpdateProductRequestDto
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public decimal? Price { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Discount price must be greater than or equal to 0")]
        public decimal? DiscountPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock must be greater than or equal to 0")]
        public int? Stock { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public Guid? CategoryId { get; set; }

        public Guid? BrandId { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsOnSale { get; set; }

        public bool? NoVoucherTag { get; set; }

        /// <summary>
        /// Optional list of images for the product. If this is null, the product's images
        /// will not be changed. If provided (even if empty), the existing images for the
        /// product will be replaced by this list.
        /// </summary>
        public List<ProductImageItemDto>? Images { get; set; }
    }
}

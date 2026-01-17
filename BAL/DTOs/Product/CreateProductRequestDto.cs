using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Product
{
    /// <summary>
    /// Create product request DTO
    /// </summary>
    public class CreateProductRequestDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Discount price must be greater than or equal to 0")]
        public decimal? DiscountPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be greater than or equal to 0")]
        public int Stock { get; set; } = 0;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public Guid? CategoryId { get; set; }

        public Guid? BrandId { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsOnSale { get; set; } = false;

        public bool NoVoucherTag { get; set; } = false;
    }
}

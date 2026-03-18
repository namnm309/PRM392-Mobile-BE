using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Product
{
    public class CreateProductVariantRequestDto
    {
        [MaxLength(100)]
        public string? Sku { get; set; }

        [MaxLength(200)]
        public string? VariantName { get; set; }

        [Required]
        [MaxLength(100)]
        public string ColorName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? ColorHex { get; set; }

        public int? RamGb { get; set; }
        public int? StorageGb { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Discount price must be greater than or equal to 0")]
        public decimal? DiscountPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be greater than or equal to 0")]
        public int Stock { get; set; } = 0;

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
    }
}


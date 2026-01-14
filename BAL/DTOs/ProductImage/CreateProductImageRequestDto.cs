using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.ProductImage
{
    /// <summary>
    /// Create product image request DTO
    /// </summary>
    public class CreateProductImageRequestDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ImageType { get; set; } = "Sub"; // Main, Sub, Poster, Thumbnail

        public int DisplayOrder { get; set; } = 0;
    }
}

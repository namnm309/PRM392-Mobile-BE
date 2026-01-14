using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.ProductImage
{
    /// <summary>
    /// Update product image request DTO
    /// </summary>
    public class UpdateProductImageRequestDto
    {
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(20)]
        public string? ImageType { get; set; }

        public int? DisplayOrder { get; set; }
    }
}

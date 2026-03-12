using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Product
{
    /// <summary>
    /// Lightweight product image item used when creating or updating a product.
    /// </summary>
    public class ProductImageItemDto
    {
        [Required]
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Image type, e.g. Main, Sub, Poster, Thumbnail.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string ImageType { get; set; } = "Sub";

        /// <summary>
        /// Display order of the image in product gallery.
        /// </summary>
        public int DisplayOrder { get; set; } = 0;
    }
}


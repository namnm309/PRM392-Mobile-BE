using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Category
{
    /// <summary>
    /// Child category item for create/bulk create requests
    /// </summary>
    public class CreateCategoryChildDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsHot { get; set; } = false;
    }
}

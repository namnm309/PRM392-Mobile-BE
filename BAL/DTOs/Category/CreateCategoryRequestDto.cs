using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Category
{
    /// <summary>
    /// Create category request DTO
    /// </summary>
    public class CreateCategoryRequestDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

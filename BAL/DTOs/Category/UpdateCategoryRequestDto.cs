using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Category
{
    /// <summary>
    /// Update category request DTO
    /// </summary>
    public class UpdateCategoryRequestDto
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }
}

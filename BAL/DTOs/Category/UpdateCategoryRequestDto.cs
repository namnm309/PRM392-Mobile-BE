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

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public int? DisplayOrder { get; set; }

        public bool? IsHot { get; set; }

        public Guid? ParentId { get; set; }
    }
}

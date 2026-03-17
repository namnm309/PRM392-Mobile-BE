using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Brand
{
    /// <summary>
    /// Update brand request DTO
    /// </summary>
    public class UpdateBrandRequestDto
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsHot { get; set; }
    }
}

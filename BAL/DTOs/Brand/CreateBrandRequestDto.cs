using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Brand
{
    /// <summary>
    /// Create brand request DTO
    /// </summary>
    public class CreateBrandRequestDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

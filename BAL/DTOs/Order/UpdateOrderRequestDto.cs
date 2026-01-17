using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Order
{
    /// <summary>
    /// Update order request DTO
    /// </summary>
    public class UpdateOrderRequestDto
    {
        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}

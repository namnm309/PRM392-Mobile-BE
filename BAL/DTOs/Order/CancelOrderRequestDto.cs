using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Order
{
    /// <summary>
    /// Cancel order request DTO
    /// </summary>
    public class CancelOrderRequestDto
    {
        [MaxLength(500)]
        public string? CancelReason { get; set; }
    }
}

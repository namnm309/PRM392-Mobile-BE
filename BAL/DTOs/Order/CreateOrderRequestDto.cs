using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Order
{
    /// <summary>
    /// Create order request DTO
    /// </summary>
    public class CreateOrderRequestDto
    {
        [Required]
        public Guid AddressId { get; set; }

        public Guid? VoucherId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public List<CreateOrderItemRequestDto> OrderItems { get; set; } = new();
    }

    /// <summary>
    /// Create order item request DTO
    /// </summary>
    public class CreateOrderItemRequestDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Cart
{
    /// <summary>
    /// DTO for updating cart item quantity
    /// </summary>
    public class UpdateCartItemRequestDto
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}

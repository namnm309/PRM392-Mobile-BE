using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Cart
{
    /// <summary>
    /// DTO for adding item to cart
    /// </summary>
    public class AddCartItemRequestDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;
    }
}

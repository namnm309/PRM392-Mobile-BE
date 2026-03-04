using System.ComponentModel.DataAnnotations;

namespace BAL.DTOs.Order
{
    /// <summary>
    /// Checkout request DTO - creates order from cart
    /// </summary>
    public class CheckoutRequestDto
    {
        [Required]
        public Guid AddressId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "COD"; // COD (Cash on Delivery), Online

        public Guid? VoucherId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// List of cart item IDs to checkout. If empty or null, checkout all items in cart.
        /// </summary>
        public List<Guid>? CartItemIds { get; set; }
    }
}

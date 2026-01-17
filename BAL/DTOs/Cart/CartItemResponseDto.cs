namespace BAL.DTOs.Cart
{
    /// <summary>
    /// Cart item response DTO with availability status
    /// </summary>
    public class CartItemResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public decimal? ProductDiscountPrice { get; set; }
        public string? ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public bool IsAvailable { get; set; }
        public int MaxQuantity { get; set; }
        public string? ReasonUnavailable { get; set; } // OUT_OF_STOCK, INACTIVE, NOT_FOUND
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

namespace BAL.DTOs.Cart
{
    /// <summary>
    /// Cart response DTO
    /// </summary>
    public class CartResponseDto
    {
        public List<CartItemResponseDto> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
    }
}

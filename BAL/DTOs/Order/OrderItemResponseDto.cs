using BAL.DTOs.Product;

namespace BAL.DTOs.Order
{
    /// <summary>
    /// Order item response DTO
    /// </summary>
    public class OrderItemResponseDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public ProductResponseDto? Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

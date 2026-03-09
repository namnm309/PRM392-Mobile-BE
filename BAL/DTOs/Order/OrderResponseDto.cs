using BAL.DTOs.Address;
using BAL.DTOs.Voucher;

namespace BAL.DTOs.Order
{
    /// <summary>
    /// Order response DTO with full details
    /// </summary>
    public class OrderResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid AddressId { get; set; }
        public AddressResponseDto? Address { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public Guid? VoucherId { get; set; }
        public VoucherResponseDto? Voucher { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public string? CancelReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        public Guid? CancelledBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string PaymentMethod { get; set; } = "COD";
        public string PaymentStatus { get; set; } = "Pending";
        public string? VnPayTransactionNo { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal ShippingFee { get; set; }
        public string? GhnOrderCode { get; set; }
        public DateTime? ExpectedDeliveryTime { get; set; }
        public int? ShippingServiceId { get; set; }
        public List<OrderItemResponseDto> OrderItems { get; set; } = new();
    }
}

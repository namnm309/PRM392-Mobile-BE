using BAL.DTOs.Common;
using BAL.DTOs.Order;

namespace BAL.Services
{
    /// <summary>
    /// Order service interface
    /// </summary>
    public interface IOrderService
    {
        Task<OrderResponseDto?> GetOrderByIdAsync(Guid id);
        Task<PagedResponse<OrderResponseDto>> GetOrdersByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<PagedResponse<OrderResponseDto>> SearchOrdersByOrderIdAsync(string orderIdSearch, int pageNumber, int pageSize);
        Task<PagedResponse<OrderResponseDto>> SearchOrdersByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<OrderResponseDto> CreateOrderAsync(Guid userId, CreateOrderRequestDto request);
        Task<OrderResponseDto> CreateOrderFromCartAsync(Guid userId, Guid addressId, string paymentMethod, List<Guid>? cartItemIds = null, Guid? voucherId = null, string? notes = null, decimal shippingFee = 0, int? shippingServiceId = null);
        Task<OrderResponseDto?> UpdateOrderAsync(Guid id, UpdateOrderRequestDto request);
        Task<bool> CancelOrderByUserAsync(Guid orderId, Guid userId);
        Task<bool> CancelOrderByStaffAsync(Guid orderId, Guid staffId, string cancelReason);
        Task<OrderResponseDto?> ConfirmOrderByAdminAsync(Guid orderId, Guid adminId, ConfirmOrderRequestDto? request = null);
        Task<OrderResponseDto?> CreateShippingOrderAsync(Guid orderId, Guid adminId);
    }
}

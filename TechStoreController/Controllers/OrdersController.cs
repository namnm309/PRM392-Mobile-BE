using BAL.DTOs.Common;
using BAL.DTOs.Order;
using BAL.Services;
using BAL.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStoreController.Helpers;
using DAL.Repositories;

namespace TechStoreController.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [AllowAnonymous]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IGhnService _ghnService;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderService orderService, 
            IGhnService ghnService, 
            IOrderRepository orderRepository,
            ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _ghnService = ghnService;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<OrderResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<OrderResponseDto>>>> GetOrders(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? userId = null)
        {
            try
            {
                var currentUserId = JwtHelper.GetUserId(User);
                if (currentUserId == null)
                    return Unauthorized(ApiResponse<PagedResponse<OrderResponseDto>>.ErrorResponse("User not authenticated"));

                var userRole = JwtHelper.GetUserRole(User);
                PagedResponse<OrderResponseDto> orders;

                // Staff/Admin can view all orders or filter by userId
                if ((userRole == "Staff" || userRole == "Admin") && userId.HasValue)
                {
                    orders = await _orderService.SearchOrdersByUserIdAsync(userId.Value, pageNumber, pageSize);
                }
                // Staff/Admin can view all orders (without userId filter)
                else if (userRole == "Staff" || userRole == "Admin")
                {
                    // For now, return empty or implement GetAllOrdersAsync if needed
                    // For simplicity, we'll use SearchByUserId with current user
                    orders = await _orderService.GetOrdersByUserIdAsync(currentUserId.Value, pageNumber, pageSize);
                }
                // Customer can only view their own orders
                else
                {
                    orders = await _orderService.GetOrdersByUserIdAsync(currentUserId.Value, pageNumber, pageSize);
                }

                return Ok(ApiResponse<PagedResponse<OrderResponseDto>>.SuccessResponse(orders, "Orders retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders");
                return StatusCode(500, ApiResponse<PagedResponse<OrderResponseDto>>.ErrorResponse("An error occurred while retrieving orders"));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<OrderResponseDto>>> GetOrder(Guid id)
        {
            try
            {
                var currentUserId = JwtHelper.GetUserId(User);
                if (currentUserId == null)
                    return Unauthorized(ApiResponse<OrderResponseDto>.ErrorResponse("User not authenticated"));

                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                    return NotFound(ApiResponse<OrderResponseDto>.ErrorResponse("Order not found"));

                // Business rule: Customer can only view their own orders
                var userRole = JwtHelper.GetUserRole(User);
                if (userRole != "Staff" && userRole != "Admin" && order.UserId != currentUserId.Value)
                {
                    return Forbid();
                }

                return Ok(ApiResponse<OrderResponseDto>.SuccessResponse(order, "Order retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId}", id);
                return StatusCode(500, ApiResponse<OrderResponseDto>.ErrorResponse("An error occurred while retrieving the order"));
            }
        }

        [HttpGet("search/by-orderid")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<OrderResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<OrderResponseDto>>>> SearchOrdersByOrderId(
            [FromQuery] string orderIdSearch,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(orderIdSearch))
                {
                    return BadRequest(ApiResponse<PagedResponse<OrderResponseDto>>.ErrorResponse("Order ID search term is required"));
                }

                var orders = await _orderService.SearchOrdersByOrderIdAsync(orderIdSearch, pageNumber, pageSize);
                return Ok(ApiResponse<PagedResponse<OrderResponseDto>>.SuccessResponse(orders, "Orders retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching orders by order ID");
                return StatusCode(500, ApiResponse<PagedResponse<OrderResponseDto>>.ErrorResponse("An error occurred while searching orders"));
            }
        }

        [HttpGet("search/by-user/{userId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<OrderResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<OrderResponseDto>>>> SearchOrdersByUserId(
            Guid userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var orders = await _orderService.SearchOrdersByUserIdAsync(userId, pageNumber, pageSize);
                return Ok(ApiResponse<PagedResponse<OrderResponseDto>>.SuccessResponse(orders, "Orders retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching orders by user ID {UserId}", userId);
                return StatusCode(500, ApiResponse<PagedResponse<OrderResponseDto>>.ErrorResponse("An error occurred while searching orders"));
            }
        }

        [HttpPost("checkout")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<OrderResponseDto>>> Checkout([FromBody] CheckoutRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<OrderResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<OrderResponseDto>.ErrorResponse("User not authenticated"));

                var order = await _orderService.CreateOrderFromCartAsync(
                    userId.Value, 
                    request.AddressId, 
                    request.PaymentMethod, 
                    request.CartItemIds, 
                    request.VoucherId, 
                    request.Notes,
                    request.ShippingFee,
                    request.ShippingServiceId);
                
                return CreatedAtAction(
                    nameof(GetOrder),
                    new { id = order.Id },
                    ApiResponse<OrderResponseDto>.SuccessResponse(order, "Order created successfully from cart")
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<OrderResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during checkout");
                return StatusCode(500, ApiResponse<OrderResponseDto>.ErrorResponse("An error occurred during checkout"));
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<OrderResponseDto>>> CreateOrder([FromBody] CreateOrderRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<OrderResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<OrderResponseDto>.ErrorResponse("User not authenticated"));

                var order = await _orderService.CreateOrderAsync(userId.Value, request);
                return CreatedAtAction(
                    nameof(GetOrder),
                    new { id = order.Id },
                    ApiResponse<OrderResponseDto>.SuccessResponse(order, "Order created successfully")
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<OrderResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, ApiResponse<OrderResponseDto>.ErrorResponse("An error occurred while creating the order"));
            }
        }

        [HttpPut("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<OrderResponseDto>>> UpdateOrder(Guid id, [FromBody] UpdateOrderRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<OrderResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var order = await _orderService.UpdateOrderAsync(id, request);
                if (order == null)
                    return NotFound(ApiResponse<OrderResponseDto>.ErrorResponse("Order not found"));

                return Ok(ApiResponse<OrderResponseDto>.SuccessResponse(order, "Order updated successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<OrderResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId}", id);
                return StatusCode(500, ApiResponse<OrderResponseDto>.ErrorResponse("An error occurred while updating the order"));
            }
        }

        [HttpPost("{id}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> CancelOrder(Guid id, [FromBody] CancelOrderRequestDto? request = null)
        {
            try
            {
                var currentUserId = JwtHelper.GetUserId(User);
                if (currentUserId == null)
                    return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

                var userRole = JwtHelper.GetUserRole(User);
                bool result;

                // Staff/Admin can cancel any order with reason
                if (userRole == "Staff" || userRole == "Admin")
                {
                    var cancelReason = request?.CancelReason ?? "Cancelled by staff/admin";
                    result = await _orderService.CancelOrderByStaffAsync(id, currentUserId.Value, cancelReason);
                }
                // Customer can only cancel their own orders with status Pending or Processing
                else
                {
                    result = await _orderService.CancelOrderByUserAsync(id, currentUserId.Value);
                }

                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResponse("Order not found or cannot be cancelled"));

                return Ok(ApiResponse<object?>.SuccessResponse(null, "Order cancelled successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while cancelling the order"));
            }
        }

        [HttpPost("{id}/confirm")]
        [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<OrderResponseDto>>> ConfirmOrder(Guid id, [FromBody] ConfirmOrderRequestDto? request = null)
        {
            try
            {
                var currentUserId = JwtHelper.GetUserId(User);
                if (currentUserId == null)
                    return Unauthorized(ApiResponse<OrderResponseDto>.ErrorResponse("User not authenticated"));

                var userRole = JwtHelper.GetUserRole(User);
                
                // Business rule: Only Staff/Admin can confirm orders
                if (userRole != "Staff" && userRole != "Admin")
                {
                    return Forbid();
                }

                var order = await _orderService.ConfirmOrderByAdminAsync(id, currentUserId.Value, request);
                
                if (order == null)
                    return NotFound(ApiResponse<OrderResponseDto>.ErrorResponse("Order not found"));

                return Ok(ApiResponse<OrderResponseDto>.SuccessResponse(order, "Order confirmed successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<OrderResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming order {OrderId}", id);
                return StatusCode(500, ApiResponse<OrderResponseDto>.ErrorResponse("An error occurred while confirming the order"));
            }
        }

        /// <summary>
        /// Tạo đơn vận chuyển GHN cho order đã confirmed
        /// </summary>
        [HttpPost("{id}/create-shipping")]
        [ProducesResponseType(typeof(ApiResponse<OrderResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<OrderResponseDto>>> CreateShippingOrder(Guid id)
        {
            try
            {
                var currentUserId = JwtHelper.GetUserId(User);
                if (currentUserId == null)
                    return Unauthorized(ApiResponse<OrderResponseDto>.ErrorResponse("User not authenticated"));

                var userRole = JwtHelper.GetUserRole(User);
                
                // Business rule: Only Staff/Admin can create shipping
                if (userRole != "Staff" && userRole != "Admin")
                {
                    return Forbid();
                }

                var order = await _orderService.CreateShippingOrderAsync(id, currentUserId.Value);
                
                if (order == null)
                    return NotFound(ApiResponse<OrderResponseDto>.ErrorResponse("Order not found"));

                return Ok(ApiResponse<OrderResponseDto>.SuccessResponse(order, "Shipping order created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<OrderResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shipping order for OrderId: {OrderId}", id);
                return StatusCode(500, ApiResponse<OrderResponseDto>.ErrorResponse("An error occurred while creating shipping order"));
            }
        }

        /// <summary>
        /// Lấy và cập nhật trạng thái đơn hàng từ GHN API (Pull model)
        /// </summary>
        [HttpGet("{id}/ghn-status")]
        [ProducesResponseType(typeof(ApiResponse<GhnStatusSyncResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<GhnStatusSyncResponseDto>>> GetGhnOrderStatus(Guid id)
        {
            try
            {
                var currentUserId = JwtHelper.GetUserId(User);
                if (currentUserId == null)
                    return Unauthorized(ApiResponse<GhnStatusSyncResponseDto>.ErrorResponse("User not authenticated"));

                // Get order from database
                var orders = await _orderRepository.FindAsync(o => o.Id == id);
                var order = orders.FirstOrDefault();

                if (order == null)
                    return NotFound(ApiResponse<GhnStatusSyncResponseDto>.ErrorResponse("Order not found"));

                // Business rule: Customer can only view their own orders
                var userRole = JwtHelper.GetUserRole(User);
                if (userRole != "Staff" && userRole != "Admin" && order.UserId != currentUserId.Value)
                {
                    return Forbid();
                }

                // Check if order has GHN order code
                if (string.IsNullOrEmpty(order.GhnOrderCode))
                {
                    return BadRequest(ApiResponse<GhnStatusSyncResponseDto>.ErrorResponse(
                        "Order does not have GHN tracking code. Only confirmed orders with shipping can be tracked."));
                }

                // Call GHN API to get order detail
                var ghnDetail = await _ghnService.GetOrderDetailAsync(order.GhnOrderCode);

                // Map GHN status to internal status
                var previousStatus = order.Status;
                var newStatus = GhnStatusMapper.MapGhnStatusToOrderStatus(ghnDetail.Status);

                // Update order if status changed
                if (previousStatus != newStatus)
                {
                    order.Status = newStatus;
                    order.UpdatedAt = DateTime.UtcNow;

                    // Update delivery timestamp if status is delivered
                    if (ghnDetail.Status.ToLower() == "delivered" && order.DeliveredAt == null)
                    {
                        order.DeliveredAt = DateTime.UtcNow;
                    }

                    // Append notes about status change
                    var statusChangeNote = $"[GHN Sync {DateTime.UtcNow:yyyy-MM-dd HH:mm}]: {previousStatus} -> {newStatus} (GHN: {ghnDetail.Status})";
                    order.Notes = string.IsNullOrEmpty(order.Notes)
                        ? statusChangeNote
                        : $"{order.Notes}\n{statusChangeNote}";

                    await _orderRepository.UpdateAsync(order);

                    _logger.LogInformation(
                        "Synced GHN status for order {OrderId}: {PreviousStatus} -> {NewStatus} (GHN: {GhnStatus})",
                        order.Id, previousStatus, newStatus, ghnDetail.Status);
                }
                else
                {
                    _logger.LogInformation(
                        "Order {OrderId} already has status {Status}, no update needed (GHN: {GhnStatus})",
                        order.Id, newStatus, ghnDetail.Status);
                }

                // Return response
                var response = new GhnStatusSyncResponseDto
                {
                    OrderId = order.Id,
                    GhnOrderCode = order.GhnOrderCode,
                    GhnStatus = ghnDetail.Status,
                    OrderStatus = newStatus,
                    PreviousOrderStatus = previousStatus,
                    StatusChanged = previousStatus != newStatus,
                    ExpectedDeliveryTime = ghnDetail.ExpectedDeliveryTime,
                    StatusLog = ghnDetail.Log.Select(log => new GhnStatusLogDto
                    {
                        Status = log.Status,
                        UpdatedDate = log.UpdatedDate,
                        Description = log.Description
                    }).ToList()
                };

                return Ok(ApiResponse<GhnStatusSyncResponseDto>.SuccessResponse(
                    response, 
                    response.StatusChanged 
                        ? "Order status updated successfully from GHN" 
                        : "Order status is already up to date"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "GHN API error for order {OrderId}", id);
                return BadRequest(ApiResponse<GhnStatusSyncResponseDto>.ErrorResponse($"GHN API error: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing GHN status for order {OrderId}", id);
                return StatusCode(500, ApiResponse<GhnStatusSyncResponseDto>.ErrorResponse("An error occurred while syncing GHN status"));
            }
        }
    }

    #region DTOs for GHN Status Sync

    public class GhnStatusSyncResponseDto
    {
        public Guid OrderId { get; set; }
        public string GhnOrderCode { get; set; } = "";
        public string GhnStatus { get; set; } = "";
        public string OrderStatus { get; set; } = "";
        public string PreviousOrderStatus { get; set; } = "";
        public bool StatusChanged { get; set; }
        public DateTime? ExpectedDeliveryTime { get; set; }
        public List<GhnStatusLogDto> StatusLog { get; set; } = new();
    }

    public class GhnStatusLogDto
    {
        public string Status { get; set; } = "";
        public DateTime UpdatedDate { get; set; }
        public string? Description { get; set; }
    }

    #endregion
}

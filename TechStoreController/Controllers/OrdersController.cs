using BAL.DTOs.Common;
using BAL.DTOs.Order;
using BAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStoreController.Helpers;

namespace TechStoreController.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
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
        [Authorize(Policy = "StaffOrAdmin")]
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
        [Authorize(Policy = "StaffOrAdmin")]
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

        [HttpPost]
        [Authorize(Policy = "CustomerOnly")]
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
        [Authorize(Policy = "StaffOrAdmin")]
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
    }
}

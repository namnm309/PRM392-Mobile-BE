using BAL.DTOs.Common;
using BAL.DTOs.Dashboard;
using BAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechStoreController.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize(Policy = "AdminOnly")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        [HttpGet("overview")]
        [ProducesResponseType(typeof(ApiResponse<DashboardOverviewResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<DashboardOverviewResponseDto>>> GetOverview(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var overview = await _dashboardService.GetOverviewAsync(start, end);
                return Ok(ApiResponse<DashboardOverviewResponseDto>.SuccessResponse(overview, "Overview retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard overview");
                return StatusCode(500, ApiResponse<DashboardOverviewResponseDto>.ErrorResponse("An error occurred while retrieving overview"));
            }
        }

        [HttpGet("top-products")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TopProductResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<TopProductResponseDto>>>> GetTopProducts(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int topN = 10)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var topProducts = await _dashboardService.GetTopProductsAsync(start, end, topN);
                return Ok(ApiResponse<IEnumerable<TopProductResponseDto>>.SuccessResponse(topProducts, "Top products retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top products");
                return StatusCode(500, ApiResponse<IEnumerable<TopProductResponseDto>>.ErrorResponse("An error occurred while retrieving top products"));
            }
        }

        [HttpGet("orders-series")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrdersSeriesResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrdersSeriesResponseDto>>>> GetOrdersSeries(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string groupBy = "day")
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                if (!new[] { "day", "week", "month" }.Contains(groupBy.ToLower()))
                {
                    return BadRequest(ApiResponse<IEnumerable<OrdersSeriesResponseDto>>.ErrorResponse("groupBy must be 'day', 'week', or 'month'"));
                }

                var series = await _dashboardService.GetOrdersSeriesAsync(start, end, groupBy);
                return Ok(ApiResponse<IEnumerable<OrdersSeriesResponseDto>>.SuccessResponse(series, "Orders series retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders series");
                return StatusCode(500, ApiResponse<IEnumerable<OrdersSeriesResponseDto>>.ErrorResponse("An error occurred while retrieving orders series"));
            }
        }

        [HttpGet("voucher-usage")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VoucherUsageResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VoucherUsageResponseDto>>>> GetVoucherUsage(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var usage = await _dashboardService.GetVoucherUsageAsync(start, end);
                return Ok(ApiResponse<IEnumerable<VoucherUsageResponseDto>>.SuccessResponse(usage, "Voucher usage retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving voucher usage");
                return StatusCode(500, ApiResponse<IEnumerable<VoucherUsageResponseDto>>.ErrorResponse("An error occurred while retrieving voucher usage"));
            }
        }
    }
}

using BAL.DTOs.Common;
using BAL.DTOs.Voucher;
using BAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStoreController.Helpers;

namespace TechStoreController.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class VouchersController : ControllerBase
    {
        private readonly IVoucherService _voucherService;
        private readonly ILogger<VouchersController> _logger;

        public VouchersController(IVoucherService voucherService, ILogger<VouchersController> logger)
        {
            _voucherService = voucherService;
            _logger = logger;
        }

        [HttpGet("{code}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<VoucherResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<VoucherResponseDto>>> GetVoucherByCode(string code)
        {
            try
            {
                var voucher = await _voucherService.GetVoucherByCodeAsync(code);
                if (voucher == null)
                    return NotFound(ApiResponse<VoucherResponseDto>.ErrorResponse("Voucher not found"));

                return Ok(ApiResponse<VoucherResponseDto>.SuccessResponse(voucher, "Voucher retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving voucher {Code}", code);
                return StatusCode(500, ApiResponse<VoucherResponseDto>.ErrorResponse("An error occurred while retrieving voucher"));
            }
        }

        [HttpPost("apply")]
        [Authorize(Policy = "CustomerOnly")]
        [ProducesResponseType(typeof(ApiResponse<VoucherBreakdownResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<VoucherBreakdownResponseDto>>> ApplyVoucher([FromBody] ApplyVoucherRequestDto request, [FromQuery] List<Guid> cartItemIds)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<VoucherBreakdownResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<VoucherBreakdownResponseDto>.ErrorResponse("User not authenticated"));

                var breakdown = await _voucherService.ApplyVoucherAsync(userId.Value, request.Code, cartItemIds);
                
                if (!string.IsNullOrEmpty(breakdown.ErrorMessage))
                {
                    return BadRequest(ApiResponse<VoucherBreakdownResponseDto>.ErrorResponse(breakdown.ErrorMessage));
                }

                return Ok(ApiResponse<VoucherBreakdownResponseDto>.SuccessResponse(breakdown, "Voucher applied successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying voucher {Code}", request.Code);
                return StatusCode(500, ApiResponse<VoucherBreakdownResponseDto>.ErrorResponse("An error occurred while applying voucher"));
            }
        }
    }
}

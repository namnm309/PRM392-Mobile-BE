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

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VoucherResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VoucherResponseDto>>>> GetVouchers(
            [FromQuery] string? code = null,
            [FromQuery] string? name = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var vouchers = await _voucherService.GetAllVouchersAsync(code, name, isActive);
                return Ok(ApiResponse<IEnumerable<VoucherResponseDto>>.SuccessResponse(vouchers, "Vouchers retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vouchers");
                return StatusCode(500, ApiResponse<IEnumerable<VoucherResponseDto>>.ErrorResponse("An error occurred while retrieving vouchers"));
            }
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<VoucherResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<VoucherResponseDto>>> GetVoucher(Guid id)
        {
            try
            {
                var voucher = await _voucherService.GetVoucherByIdAsync(id);
                if (voucher == null)
                    return NotFound(ApiResponse<VoucherResponseDto>.ErrorResponse("Voucher not found"));

                return Ok(ApiResponse<VoucherResponseDto>.SuccessResponse(voucher, "Voucher retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving voucher {VoucherId}", id);
                return StatusCode(500, ApiResponse<VoucherResponseDto>.ErrorResponse("An error occurred while retrieving voucher"));
            }
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

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<VoucherResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<VoucherResponseDto>>> CreateVoucher([FromBody] CreateVoucherRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<VoucherResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var voucher = await _voucherService.CreateVoucherAsync(request);
                return CreatedAtAction(
                    nameof(GetVoucher),
                    new { id = voucher.Id },
                    ApiResponse<VoucherResponseDto>.SuccessResponse(voucher, "Voucher created successfully")
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<VoucherResponseDto>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<VoucherResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating voucher");
                return StatusCode(500, ApiResponse<VoucherResponseDto>.ErrorResponse("An error occurred while creating voucher"));
            }
        }

        [HttpPut("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<VoucherResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<VoucherResponseDto>>> UpdateVoucher(Guid id, [FromBody] UpdateVoucherRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<VoucherResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var voucher = await _voucherService.UpdateVoucherAsync(id, request);
                if (voucher == null)
                    return NotFound(ApiResponse<VoucherResponseDto>.ErrorResponse("Voucher not found"));

                return Ok(ApiResponse<VoucherResponseDto>.SuccessResponse(voucher, "Voucher updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<VoucherResponseDto>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<VoucherResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating voucher {VoucherId}", id);
                return StatusCode(500, ApiResponse<VoucherResponseDto>.ErrorResponse("An error occurred while updating voucher"));
            }
        }

        [HttpDelete("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteVoucher(Guid id)
        {
            try
            {
                var result = await _voucherService.DeleteVoucherAsync(id);
                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResponse("Voucher not found"));

                return Ok(ApiResponse<object?>.SuccessResponse(null, "Voucher deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting voucher {VoucherId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while deleting voucher"));
            }
        }

        [HttpPost("{id}/toggle-active")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<VoucherResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<VoucherResponseDto>>> ToggleActive(Guid id)
        {
            try
            {
                var voucher = await _voucherService.ToggleActiveAsync(id);
                if (voucher == null)
                    return NotFound(ApiResponse<VoucherResponseDto>.ErrorResponse("Voucher not found"));

                return Ok(ApiResponse<VoucherResponseDto>.SuccessResponse(voucher, "Voucher status toggled successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling voucher status {VoucherId}", id);
                return StatusCode(500, ApiResponse<VoucherResponseDto>.ErrorResponse("An error occurred while toggling voucher status"));
            }
        }

        [HttpPost("apply")]
        [AllowAnonymous]
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

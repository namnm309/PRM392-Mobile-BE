using BAL.DTOs.Address;
using BAL.DTOs.Common;
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
    public class AddressesController : ControllerBase
    {
        private readonly IAddressService _addressService;
        private readonly ILogger<AddressesController> _logger;

        public AddressesController(IAddressService addressService, ILogger<AddressesController> logger)
        {
            _addressService = addressService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AddressResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<AddressResponseDto>>>> GetMyAddresses()
        {
            try
            {
                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<IEnumerable<AddressResponseDto>>.ErrorResponse("User not authenticated"));

                var addresses = await _addressService.GetUserAddressesAsync(userId.Value);
                return Ok(ApiResponse<IEnumerable<AddressResponseDto>>.SuccessResponse(addresses, "Addresses retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving addresses");
                return StatusCode(500, ApiResponse<IEnumerable<AddressResponseDto>>.ErrorResponse("An error occurred while retrieving addresses"));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AddressResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AddressResponseDto>>> GetAddress(Guid id)
        {
            try
            {
                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<AddressResponseDto>.ErrorResponse("User not authenticated"));

                var address = await _addressService.GetAddressByIdAsync(id, userId.Value);
                if (address == null)
                    return NotFound(ApiResponse<AddressResponseDto>.ErrorResponse("Address not found"));

                return Ok(ApiResponse<AddressResponseDto>.SuccessResponse(address, "Address retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving address {AddressId}", id);
                return StatusCode(500, ApiResponse<AddressResponseDto>.ErrorResponse("An error occurred while retrieving the address"));
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AddressResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AddressResponseDto>>> CreateAddress([FromBody] CreateAddressRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<AddressResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<AddressResponseDto>.ErrorResponse("User not authenticated"));

                var address = await _addressService.CreateAddressAsync(userId.Value, request);
                return CreatedAtAction(
                    nameof(GetAddress),
                    new { id = address.Id },
                    ApiResponse<AddressResponseDto>.SuccessResponse(address, "Address created successfully")
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AddressResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address");
                return StatusCode(500, ApiResponse<AddressResponseDto>.ErrorResponse("An error occurred while creating the address"));
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AddressResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AddressResponseDto>>> UpdateAddress(Guid id, [FromBody] UpdateAddressRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<AddressResponseDto>.ErrorResponse("Validation failed", errors));
                }

                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<AddressResponseDto>.ErrorResponse("User not authenticated"));

                var address = await _addressService.UpdateAddressAsync(id, userId.Value, request);
                if (address == null)
                    return NotFound(ApiResponse<AddressResponseDto>.ErrorResponse("Address not found"));

                return Ok(ApiResponse<AddressResponseDto>.SuccessResponse(address, "Address updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId}", id);
                return StatusCode(500, ApiResponse<AddressResponseDto>.ErrorResponse("An error occurred while updating the address"));
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteAddress(Guid id)
        {
            try
            {
                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

                var result = await _addressService.DeleteAddressAsync(id, userId.Value);
                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResponse("Address not found"));

                return Ok(ApiResponse<object?>.SuccessResponse(null, "Address deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while deleting the address"));
            }
        }

        [HttpPost("{id}/set-primary")]
        [ProducesResponseType(typeof(ApiResponse<AddressResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AddressResponseDto>>> SetPrimaryAddress(Guid id)
        {
            try
            {
                var userId = JwtHelper.GetUserId(User);
                if (userId == null)
                    return Unauthorized(ApiResponse<AddressResponseDto>.ErrorResponse("User not authenticated"));

                var address = await _addressService.SetPrimaryAddressAsync(id, userId.Value);
                return Ok(ApiResponse<AddressResponseDto>.SuccessResponse(address, "Primary address updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AddressResponseDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary address {AddressId}", id);
                return StatusCode(500, ApiResponse<AddressResponseDto>.ErrorResponse("An error occurred while setting primary address"));
            }
        }
    }
}

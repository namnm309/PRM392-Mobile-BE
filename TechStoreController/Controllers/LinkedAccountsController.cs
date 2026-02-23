using BAL.DTOs.Common;
using BAL.DTOs.LinkedAccount;
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
    public class LinkedAccountsController : ControllerBase
    {
        private readonly ILinkedAccountService _linkedAccountService;
        private readonly ILogger<LinkedAccountsController> _logger;

        public LinkedAccountsController(ILinkedAccountService linkedAccountService, ILogger<LinkedAccountsController> logger)
        {
            _linkedAccountService = linkedAccountService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<LinkedAccountsResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<LinkedAccountsResponse>>> GetLinkedAccounts()
        {
            var userId = JwtHelper.GetUserId(User);
            if (userId == null)
                return Unauthorized(ApiResponse<LinkedAccountsResponse>.ErrorResponse("User not authenticated"));

            var response = await _linkedAccountService.GetLinkedAccountsAsync(userId.Value);
            return Ok(ApiResponse<LinkedAccountsResponse>.SuccessResponse(response, "Linked accounts retrieved"));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LinkedAccountDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<LinkedAccountDto>>> LinkAccount([FromBody] LinkAccountRequest request)
        {
            var userId = JwtHelper.GetUserId(User);
            if (userId == null)
                return Unauthorized(ApiResponse<LinkedAccountDto>.ErrorResponse("User not authenticated"));

            try
            {
                var account = await _linkedAccountService.LinkAccountAsync(userId.Value, request);
                return CreatedAtAction(nameof(GetLinkedAccounts), null, ApiResponse<LinkedAccountDto>.SuccessResponse(account, $"{request.Provider} account linked"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<LinkedAccountDto>.ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<LinkedAccountDto>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<LinkedAccountDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{provider}")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object?>>> UnlinkAccount(string provider)
        {
            var userId = JwtHelper.GetUserId(User);
            if (userId == null)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            var result = await _linkedAccountService.UnlinkAccountAsync(userId.Value, provider);
            if (!result)
                return NotFound(ApiResponse<object>.ErrorResponse($"No {provider} account linked"));

            return Ok(ApiResponse<object?>.SuccessResponse(null, $"{provider} account unlinked"));
        }
    }
}

using BAL.DTOs.Common;
using BAL.DTOs.Membership;
using BAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStoreController.Helpers;

namespace TechStoreController.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MembershipController : ControllerBase
    {
        private readonly IMembershipService _membershipService;
        private readonly ILogger<MembershipController> _logger;

        public MembershipController(IMembershipService membershipService, ILogger<MembershipController> logger)
        {
            _membershipService = membershipService;
            _logger = logger;
        }

        [HttpGet("tiers")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<MembershipTierDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IEnumerable<MembershipTierDto>>>> GetAllTiers()
        {
            var tiers = await _membershipService.GetAllTiersAsync();
            return Ok(ApiResponse<IEnumerable<MembershipTierDto>>.SuccessResponse(tiers, "Membership tiers retrieved"));
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserMembershipDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<UserMembershipDto>>> GetMyMembership()
        {
            var userId = JwtHelper.GetUserId(User);
            if (userId == null)
                return Unauthorized(ApiResponse<UserMembershipDto>.ErrorResponse("User not authenticated"));

            try
            {
                var membership = await _membershipService.GetUserMembershipAsync(userId.Value);
                return Ok(ApiResponse<UserMembershipDto>.SuccessResponse(membership, "Membership info retrieved"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<UserMembershipDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("points/history")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PointHistoryResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PointHistoryResponse>>> GetPointHistory([FromQuery] int limit = 20)
        {
            var userId = JwtHelper.GetUserId(User);
            if (userId == null)
                return Unauthorized(ApiResponse<PointHistoryResponse>.ErrorResponse("User not authenticated"));

            var history = await _membershipService.GetUserPointHistoryAsync(userId.Value, limit);
            return Ok(ApiResponse<PointHistoryResponse>.SuccessResponse(history, "Point history retrieved"));
        }

        [HttpPost("tiers")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<MembershipTierDto>), StatusCodes.Status201Created)]
        public async Task<ActionResult<ApiResponse<MembershipTierDto>>> CreateTier([FromBody] CreateMembershipTierRequest request)
        {
            var tier = await _membershipService.CreateTierAsync(request);
            return CreatedAtAction(nameof(GetAllTiers), null, ApiResponse<MembershipTierDto>.SuccessResponse(tier, "Membership tier created"));
        }

        [HttpPut("tiers/{tierId}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<MembershipTierDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<MembershipTierDto>>> UpdateTier(Guid tierId, [FromBody] UpdateMembershipTierRequest request)
        {
            var tier = await _membershipService.UpdateTierAsync(tierId, request);
            if (tier == null)
                return NotFound(ApiResponse<MembershipTierDto>.ErrorResponse("Membership tier not found"));

            return Ok(ApiResponse<MembershipTierDto>.SuccessResponse(tier, "Membership tier updated"));
        }

        [HttpDelete("tiers/{tierId}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object?>>> DeleteTier(Guid tierId)
        {
            var result = await _membershipService.DeleteTierAsync(tierId);
            if (!result)
                return NotFound(ApiResponse<object>.ErrorResponse("Membership tier not found"));

            return Ok(ApiResponse<object?>.SuccessResponse(null, "Membership tier deleted"));
        }

        [HttpPost("points")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<PointTransactionDto>), StatusCodes.Status201Created)]
        public async Task<ActionResult<ApiResponse<PointTransactionDto>>> AddPoints([FromBody] AddPointsRequest request)
        {
            try
            {
                var transaction = await _membershipService.AddPointsAsync(request);
                return CreatedAtAction(nameof(GetPointHistory), null, ApiResponse<PointTransactionDto>.SuccessResponse(transaction, "Points added"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<PointTransactionDto>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("users/{userId}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<UserMembershipDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserMembershipDto>>> GetUserMembership(Guid userId)
        {
            try
            {
                var membership = await _membershipService.GetUserMembershipAsync(userId);
                return Ok(ApiResponse<UserMembershipDto>.SuccessResponse(membership, "User membership info retrieved"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<UserMembershipDto>.ErrorResponse(ex.Message));
            }
        }
    }
}

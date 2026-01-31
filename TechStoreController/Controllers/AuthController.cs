using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStoreController.Services;

namespace TechStoreController.Controllers;

/// <summary>
/// Auth endpoints: dev token (lấy JWT từ Clerk theo userId để test Swagger).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IClerkBackendApiService _clerkBackendApi;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IClerkBackendApiService clerkBackendApi,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _clerkBackendApi = clerkBackendApi;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// [Dev] Lấy JWT cho user từ Clerk (để dán vào Swagger Authorize).
    /// Lấy userId từ Clerk Dashboard → Users → chọn user → copy User ID (user_xxx).
    /// User phải có ít nhất một session active (đã đăng nhập từ app trước đó).
    /// </summary>
    [HttpGet("dev/token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DevTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<DevTokenResponse>> GetDevToken([FromQuery] string? userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new { error = "userId is required (Clerk User ID, e.g. user_xxx). Get it from Clerk Dashboard → Users." });
        }

        var secretKey = _configuration["Clerk:SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            _logger.LogWarning("Clerk:SecretKey is not configured. Add it to appsettings for dev token endpoint.");
            return StatusCode(503, new { error = "Clerk:SecretKey is not configured. Add to appsettings (or env) for this endpoint." });
        }

        var jwt = await _clerkBackendApi.GetSessionTokenForUserAsync(userId.Trim(), cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(jwt))
        {
            return NotFound(new
            {
                error = "No active session for this user. User must sign in at least once from the app, then try again.",
                userId = userId
            });
        }

        return Ok(new DevTokenResponse { Jwt = jwt });
    }

    public class DevTokenResponse
    {
        public string Jwt { get; set; } = string.Empty;
    }
}

using System.Text.Json;

namespace TechStoreController.Services;

/// <summary>
/// Gọi Clerk Backend API qua HTTP. Cần cấu hình Clerk:SecretKey và Clerk:BackendApiUrl (mặc định https://api.clerk.com).
/// </summary>
public class ClerkBackendApiService : IClerkBackendApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClerkBackendApiService> _logger;

    public ClerkBackendApiService(HttpClient httpClient, ILogger<ClerkBackendApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> GetSessionTokenForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("GetSessionTokenForUserAsync: userId is empty");
            return null;
        }

        // GET /v1/sessions?user_id=xxx&status=active
        var sessionsUrl = $"sessions?user_id={Uri.EscapeDataString(userId)}&status=active&limit=1";
        using var sessionsResponse = await _httpClient.GetAsync(sessionsUrl, cancellationToken).ConfigureAwait(false);
        if (!sessionsResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning("Clerk sessions list failed: {StatusCode} for userId {UserId}", sessionsResponse.StatusCode, userId);
            return null;
        }

        var sessionsJson = await sessionsResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        using var sessionsDoc = JsonDocument.Parse(sessionsJson);
        var root = sessionsDoc.RootElement;
        if (!root.TryGetProperty("data", out var data) || data.GetArrayLength() == 0)
        {
            _logger.LogWarning("No active session for user {UserId}. User must sign in at least once from app.", userId);
            return null;
        }

        var sessionId = data[0].GetProperty("id").GetString();
        if (string.IsNullOrEmpty(sessionId))
        {
            _logger.LogWarning("Clerk session id missing for user {UserId}", userId);
            return null;
        }

        // POST /v1/sessions/{session_id}/tokens/default
        var tokenUrl = $"sessions/{sessionId}/tokens/default";
        using var tokenResponse = await _httpClient.PostAsync(tokenUrl, null, cancellationToken).ConfigureAwait(false);
        if (!tokenResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning("Clerk get token failed: {StatusCode} for sessionId {SessionId}", tokenResponse.StatusCode, sessionId);
            return null;
        }

        var tokenJson = await tokenResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        using var tokenDoc = JsonDocument.Parse(tokenJson);
        var jwt = tokenDoc.RootElement.TryGetProperty("jwt", out var jwtEl) ? jwtEl.GetString() : null;
        if (!string.IsNullOrEmpty(jwt))
            _logger.LogInformation("Issued dev JWT for userId {UserId}, sessionId {SessionId}", userId, sessionId);
        return jwt;
    }
}

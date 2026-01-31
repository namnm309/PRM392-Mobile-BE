namespace TechStoreController.Services;

/// <summary>
/// Gọi Clerk Backend API (list sessions, get session token) để phục vụ dev/test (lấy JWT từ userId).
/// </summary>
public interface IClerkBackendApiService
{
    /// <summary>
    /// Lấy session JWT cho user (user phải có ít nhất một session active - đã đăng nhập từ app trước đó).
    /// Gọi Clerk: GET sessions?user_id= → POST sessions/{id}/tokens/default.
    /// </summary>
    Task<string?> GetSessionTokenForUserAsync(string userId, CancellationToken cancellationToken = default);
}

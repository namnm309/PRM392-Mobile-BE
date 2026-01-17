using System.Security.Claims;

namespace TechStoreController.Helpers
{
    /// <summary>
    /// Helper class for JWT operations
    /// </summary>
    public static class JwtHelper
    {
        public const string RoleGuest = "Guest";
        public const string RoleCustomer = "Customer";
        public const string RoleStaff = "Staff";
        public const string RoleAdmin = "Admin";

        /// <summary>
        /// Get user ID from JWT claims
        /// </summary>
        public static Guid? GetUserId(ClaimsPrincipal? user)
        {
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? user?.FindFirst("sub")?.Value
                           ?? user?.FindFirst("userId")?.Value;
            
            if (Guid.TryParse(userIdClaim, out var userId))
                return userId;
            
            return null;
        }

        /// <summary>
        /// Get user role from JWT claims
        /// </summary>
        public static string? GetUserRole(ClaimsPrincipal? user)
        {
            return user?.FindFirst(ClaimTypes.Role)?.Value 
                ?? user?.FindFirst("role")?.Value;
        }

        /// <summary>
        /// Check if user has required role
        /// </summary>
        public static bool HasRole(ClaimsPrincipal? user, params string[] roles)
        {
            var userRole = GetUserRole(user);
            return userRole != null && roles.Contains(userRole);
        }
    }
}

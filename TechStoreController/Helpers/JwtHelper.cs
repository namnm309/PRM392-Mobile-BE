using System.Security.Claims;

namespace TechStoreController.Helpers
{
    /// <summary>
    /// Helper to read userId and role from claims (set after Clerk token validation in OnTokenValidated).
    /// </summary>
    public static class JwtHelper
    {
        public const string RoleGuest = "Guest";
        public const string RoleCustomer = "Customer";
        public const string RoleStaff = "Staff";
        public const string RoleAdmin = "Admin";

        /// <summary>
        /// Get user ID from JWT claims (prefer backend userId Guid added in OnTokenValidated).
        /// </summary>
        public static Guid? GetUserId(ClaimsPrincipal? user)
        {
            var userIdClaim = user?.FindFirst("userId")?.Value
                           ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? user?.FindFirst("sub")?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
                return userId;

            return null;
        }

        /// <summary>
        /// Get Clerk ID from JWT claims (sub or clerkId added in OnTokenValidated).
        /// </summary>
        public static string? GetClerkId(ClaimsPrincipal? user)
        {
            return user?.FindFirst("clerkId")?.Value
                ?? user?.FindFirst("sub")?.Value;
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

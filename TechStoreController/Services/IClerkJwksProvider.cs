using Microsoft.IdentityModel.Tokens;

namespace TechStoreController.Services
{
    /// <summary>
    /// Provides Clerk JWKS signing keys for JWT Bearer validation. Caches keys and refreshes periodically.
    /// </summary>
    public interface IClerkJwksProvider
    {
        /// <summary>
        /// Returns the current signing keys from Clerk JWKS (cached). Used by AddJwtBearer IssuerSigningKeyResolver.
        /// </summary>
        IEnumerable<SecurityKey> GetSigningKeys();
    }
}

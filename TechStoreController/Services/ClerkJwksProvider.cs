using Microsoft.IdentityModel.Tokens;

namespace TechStoreController.Services
{
    /// <summary>
    /// Fetches and caches Clerk JWKS; provides signing keys for JWT Bearer validation.
    /// </summary>
    public class ClerkJwksProvider : IClerkJwksProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _jwksUrl;
        private readonly ILogger<ClerkJwksProvider> _logger;
        private JsonWebKeySet? _cachedJwks;
        private DateTime _cacheExpiry = DateTime.MinValue;
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(15);
        private readonly SemaphoreSlim _loadLock = new(1, 1);

        public ClerkJwksProvider(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<ClerkJwksProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _jwksUrl = configuration["Clerk:JwksUrl"] ?? string.Empty;
            _logger = logger;
        }

        public IEnumerable<SecurityKey> GetSigningKeys()
        {
            EnsureLoaded();
            if (_cachedJwks == null)
                return Array.Empty<SecurityKey>();
            return _cachedJwks.GetSigningKeys();
        }

        private void EnsureLoaded()
        {
            if (_cachedJwks != null && DateTime.UtcNow < _cacheExpiry)
                return;
            _loadLock.Wait();
            try
            {
                if (_cachedJwks != null && DateTime.UtcNow < _cacheExpiry)
                    return;
                if (string.IsNullOrWhiteSpace(_jwksUrl))
                {
                    _logger.LogWarning("Clerk:JwksUrl is not configured");
                    return;
                }
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    var jwksJson = client.GetStringAsync(_jwksUrl).GetAwaiter().GetResult();
                    _cachedJwks = new JsonWebKeySet(jwksJson);
                    _cacheExpiry = DateTime.UtcNow.Add(CacheTtl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to fetch Clerk JWKS from {JwksUrl}", _jwksUrl);
                }
            }
            finally
            {
                _loadLock.Release();
            }
        }
    }
}

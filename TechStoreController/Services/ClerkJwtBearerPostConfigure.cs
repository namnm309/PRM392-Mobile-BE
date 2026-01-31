using System.Security.Claims;
using System.Threading.Tasks;
using BAL.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace TechStoreController.Services
{
    /// <summary>
    /// Configures JwtBearerOptions to validate Clerk JWT (JWKS) and map sub (ClerkId) to userId and role.
    /// </summary>
    public class ClerkJwtBearerPostConfigure : IPostConfigureOptions<JwtBearerOptions>
    {
        private readonly IServiceProvider _serviceProvider;

        public ClerkJwtBearerPostConfigure(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void PostConfigure(string? name, JwtBearerOptions options)
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                {
                    var jwksProvider = _serviceProvider.GetRequiredService<IClerkJwksProvider>();
                    return jwksProvider.GetSigningKeys();
                },
                RequireSignedTokens = true,
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger<ClerkJwtBearerPostConfigure>)) as ILogger<ClerkJwtBearerPostConfigure>;
                    logger?.LogWarning("JWT validation failed: {Message}", context.Exception?.Message ?? "Unknown");
                    return Task.CompletedTask;
                },
                OnTokenValidated = async context =>
                {
                    var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger<ClerkJwtBearerPostConfigure>)) as ILogger<ClerkJwtBearerPostConfigure>;
                    var sub = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? context.Principal?.FindFirst("sub")?.Value;
                    if (string.IsNullOrEmpty(sub))
                    {
                        logger?.LogWarning("JWT missing sub claim");
                        context.Fail("Missing sub claim");
                        return;
                    }

                    using var scope = _serviceProvider.CreateScope();
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                    var user = await userService.GetUserByClerkIdAsync(sub);
                    if (user == null)
                    {
                        logger?.LogWarning("User not found for ClerkId: {ClerkId}. Create user via webhook or POST /api/Users first.", sub);
                        context.Fail("User not found for ClerkId");
                        return;
                    }

                    var identity = context.Principal?.Identity as ClaimsIdentity;
                    if (identity != null)
                    {
                        identity.AddClaim(new Claim("userId", user.Id.ToString()));
                        identity.AddClaim(new Claim("clerkId", sub));
                        identity.AddClaim(new Claim(ClaimTypes.Role, user.Role));
                        identity.AddClaim(new Claim("role", user.Role));
                    }
                },
            };
        }
    }
}

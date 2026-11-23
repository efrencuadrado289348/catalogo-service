using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Catalog.API.Security;

public static class JwtAuthenticationExtensions
{
    /// <summary>
    /// Validates the JWT issued by Identity.API: signature, issuer, audience and lifetime.
    /// Validation is local (shared HS256 key); Catalog never calls Identity at runtime.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var issuer = Required(configuration, "Jwt:Issuer");
        var audience = Required(configuration, "Jwt:Audience");
        var key = Required(configuration, "Jwt:Key");

        if (Encoding.UTF8.GetByteCount(key) < 32)
            throw new InvalidOperationException("'Jwt:Key' must be at least 32 bytes for HS256.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep claim names as Identity issues them ("sub", "role", "name")
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero, // expires exactly at "exp" (default tolerance is 5 min)
                    RoleClaimType = "role",
                    NameClaimType = "name"
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static string Required(IConfiguration configuration, string name) =>
        configuration[name] is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"Missing configuration value '{name}'.");
}

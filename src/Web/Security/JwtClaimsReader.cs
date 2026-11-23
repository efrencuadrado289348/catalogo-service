using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace Web.Security;

public sealed record JwtInfo(IReadOnlyList<Claim> Claims, DateTimeOffset ExpiresAt);

/// <summary>
/// Reads the claims of the JWT returned by Identity to build the session (name, email, roles, expiration).
/// The signature is not validated here on purpose: Web received the token directly from Identity
/// in a server-to-server call, it is not user input. The APIs (Catalog, Orders) validate it on every request.
/// </summary>
public static class JwtClaimsReader
{
    public static JwtInfo Read(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
            throw new FormatException("El token recibido no tiene formato JWT.");

        using var payload = JsonDocument.Parse(WebEncoders.Base64UrlDecode(parts[1]));
        var root = payload.RootElement;

        var claims = new List<Claim>();
        AddIfPresent(claims, root, "sub", ClaimTypes.NameIdentifier);
        AddIfPresent(claims, root, "email", ClaimTypes.Email);

        var name = GetString(root, "name");
        claims.Add(new Claim(ClaimTypes.Name, string.IsNullOrEmpty(name) ? GetString(root, "email") ?? "usuario" : name));

        // "role" is a string with one role, or an array with several
        if (root.TryGetProperty("role", out var role))
        {
            var roles = new List<string?>();
            if (role.ValueKind == JsonValueKind.Array)
                roles.AddRange(role.EnumerateArray().Select(r => r.GetString()));
            else
                roles.Add(role.GetString());

            claims.AddRange(roles.OfType<string>().Select(r => new Claim(ClaimTypes.Role, r)));
        }

        var expiresAt = root.TryGetProperty("exp", out var exp)
            ? DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64())
            : DateTimeOffset.UtcNow.AddMinutes(30);

        return new JwtInfo(claims, expiresAt);
    }

    private static void AddIfPresent(List<Claim> claims, JsonElement root, string jwtName, string claimType)
    {
        var value = GetString(root, jwtName);
        if (!string.IsNullOrEmpty(value))
            claims.Add(new Claim(claimType, value));
    }

    private static string? GetString(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
}

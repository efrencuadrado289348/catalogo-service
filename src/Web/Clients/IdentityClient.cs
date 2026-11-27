using System.Net;

namespace Web.Clients;

public sealed record LoginResult(string AccessToken, string TokenType, int ExpiresIn);

public sealed class IdentityClient(HttpClient http)
{
    /// <summary>Returns null when the credentials are invalid.</summary>
    public async Task<LoginResult?> LoginAsync(string email, string password, CancellationToken ct)
    {
        using var response = await http.PostAsJsonAsync("api/auth/login", new { email, password }, ct);

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LoginResult>(ct);
    }
}

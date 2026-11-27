using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace Web.Security;

/// <summary>
/// Adds "Authorization: Bearer &lt;jwt&gt;" to calls made to Catalog and Orders.
/// The JWT lives in the user's session cookie (server side), never in the browser's JavaScript.
/// </summary>
public sealed class BearerTokenHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    public const string TokenName = "access_token";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var token = httpContext is null ? null : await httpContext.GetTokenAsync(TokenName);

        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}

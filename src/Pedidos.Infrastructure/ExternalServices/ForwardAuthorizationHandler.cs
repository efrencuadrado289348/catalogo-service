using Microsoft.AspNetCore.Http;

namespace Pedidos.Infrastructure.ExternalServices;

/// <summary>
/// Token propagation: forwards the caller's JWT ("Authorization: Bearer ...") to Catalog,
/// so Pedidos calls Catalog on behalf of the authenticated user.
/// </summary>
public sealed class ForwardAuthorizationHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authorization = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

        if (!string.IsNullOrEmpty(authorization))
            request.Headers.TryAddWithoutValidation("Authorization", authorization);

        return base.SendAsync(request, cancellationToken);
    }
}

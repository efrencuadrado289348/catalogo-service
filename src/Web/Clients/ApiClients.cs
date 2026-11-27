using Microsoft.Extensions.Options;
using Web.Options;
using Web.Security;

namespace Web.Clients;

/// <summary>
/// Named HttpClients, one per microservice. Typed clients reuse them:
///   builder.Services.AddHttpClient&lt;CatalogClient&gt;(ApiClients.Catalog);
/// Catalog and Orders automatically send the user's JWT (BearerTokenHandler).
/// </summary>
public static class ApiClients
{
    public const string Identity = "Identity";
    public const string Catalog = "Catalog";
    public const string Orders = "Orders";

    public static IServiceCollection AddApiClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ServiceUrls>()
            .Bind(configuration.GetSection(ServiceUrls.SectionName))
            .Validate(u => IsAbsoluteUrl(u.IdentityBaseUrl) && IsAbsoluteUrl(u.CatalogBaseUrl) && IsAbsoluteUrl(u.OrdersBaseUrl),
                "Services:IdentityBaseUrl, Services:CatalogBaseUrl y Services:OrdersBaseUrl deben ser URL absolutas.")
            .ValidateOnStart();

        services.AddHttpContextAccessor();
        services.AddTransient<BearerTokenHandler>();

        // Identity: login is anonymous, no token
        services.AddHttpClient(Identity, (sp, client) =>
            client.BaseAddress = BaseAddress(sp.GetRequiredService<IOptions<ServiceUrls>>().Value.IdentityBaseUrl));

        services.AddHttpClient(Catalog, (sp, client) =>
                client.BaseAddress = BaseAddress(sp.GetRequiredService<IOptions<ServiceUrls>>().Value.CatalogBaseUrl))
            .AddHttpMessageHandler<BearerTokenHandler>();

        services.AddHttpClient(Orders, (sp, client) =>
                client.BaseAddress = BaseAddress(sp.GetRequiredService<IOptions<ServiceUrls>>().Value.OrdersBaseUrl))
            .AddHttpMessageHandler<BearerTokenHandler>();

        return services;
    }

    // Trailing slash so relative paths ("api/products") are appended, not replaced
    private static Uri BaseAddress(string url) => new(url.EndsWith('/') ? url : url + "/");

    private static bool IsAbsoluteUrl(string url) => Uri.TryCreate(url, UriKind.Absolute, out _);
}

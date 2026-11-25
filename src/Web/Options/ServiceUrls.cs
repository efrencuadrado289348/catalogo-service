namespace Web.Options;

/// <summary>Base URLs of the microservices. Section "Services" in appsettings / environment variables.</summary>
public sealed class ServiceUrls
{
    public const string SectionName = "Services";

    public string IdentityBaseUrl { get; init; } = string.Empty;
    public string CatalogBaseUrl { get; init; } = string.Empty;
    public string OrdersBaseUrl { get; init; } = string.Empty;
}

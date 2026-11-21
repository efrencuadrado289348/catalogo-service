namespace Catalog.API.Security;

/// <summary>
/// Role names issued by Identity.API (see docs/contrato-identidad.md). Case sensitive.
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Vendedor = "Vendedor";
    public const string Cliente = "Cliente";

    /// <summary>Comma separated list for [Authorize(Roles = ...)]: any of them grants access.</summary>
    public const string AdminOrVendedor = $"{Admin},{Vendedor}";
}

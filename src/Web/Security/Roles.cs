namespace Web.Security;

/// <summary>Role names issued by Identity.API (docs/contrato-identidad.md). Case sensitive.</summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Vendedor = "Vendedor";
    public const string Cliente = "Cliente";

    /// <summary>For [Authorize(Roles = ...)]: any of them grants access.</summary>
    public const string AdminOrVendedor = $"{Admin},{Vendedor}";
}

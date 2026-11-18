namespace Identity.API.Data.Seeding;

/// <summary>Role names agreed by the team (see docs/contrato-identidad.md). Case sensitive.</summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Vendedor = "Vendedor";
    public const string Cliente = "Cliente";

    public static readonly IReadOnlyList<string> All = [Admin, Vendedor, Cliente];
}

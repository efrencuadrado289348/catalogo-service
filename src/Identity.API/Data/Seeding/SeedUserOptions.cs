namespace Identity.API.Data.Seeding;

/// <summary>
/// Initial user for one role. Bound from configuration section "SeedUsers:{Role}".
/// Environment variable example: SeedUsers__Admin__Password.
/// </summary>
public sealed class SeedUserOptions
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

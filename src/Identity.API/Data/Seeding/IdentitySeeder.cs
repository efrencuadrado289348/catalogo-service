using Microsoft.AspNetCore.Identity;

namespace Identity.API.Data.Seeding;

/// <summary>
/// Creates the initial roles and users. It is idempotent: running it again never
/// duplicates data and never resets the password of an existing user.
/// Passwords are hashed by ASP.NET Core Identity (UserManager); they are never stored in plain text.
/// </summary>
public sealed class IdentitySeeder(
    RoleManager<IdentityRole<Guid>> roleManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    ILogger<IdentitySeeder> logger)
{
    public async Task SeedAsync()
    {
        foreach (var roleName in Roles.All)
        {
            await EnsureRoleAsync(roleName);
        }

        foreach (var roleName in Roles.All)
        {
            var options = configuration.GetSection($"SeedUsers:{roleName}").Get<SeedUserOptions>();
            await EnsureUserAsync(roleName, options);
        }
    }

    private async Task EnsureRoleAsync(string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            logger.LogInformation("Seed: role '{Role}' already exists.", roleName);
            return;
        }

        var result = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        ThrowIfFailed(result, $"create role '{roleName}'");
        logger.LogInformation("Seed: role '{Role}' created.", roleName);
    }

    private async Task EnsureUserAsync(string roleName, SeedUserOptions? options)
    {
        if (string.IsNullOrWhiteSpace(options?.Email) || string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException(
                $"Seed configuration for role '{roleName}' is incomplete. " +
                $"Set 'SeedUsers:{roleName}:Email' and 'SeedUsers:{roleName}:Password'.");
        }

        var user = await userManager.FindByEmailAsync(options.Email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = options.Email,
                Email = options.Email,
                EmailConfirmed = true
            };

            var created = await userManager.CreateAsync(user, options.Password);
            ThrowIfFailed(created, $"create user '{options.Email}'");
            logger.LogInformation("Seed: user '{Email}' created.", options.Email);
        }
        else
        {
            logger.LogInformation("Seed: user '{Email}' already exists.", options.Email);
        }

        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            var added = await userManager.AddToRoleAsync(user, roleName);
            ThrowIfFailed(added, $"assign role '{roleName}' to '{options.Email}'");
            logger.LogInformation("Seed: user '{Email}' assigned to role '{Role}'.", options.Email, roleName);
        }
    }

    private static void ThrowIfFailed(IdentityResult result, string action)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
        throw new InvalidOperationException($"Seed failed to {action}: {errors}");
    }
}

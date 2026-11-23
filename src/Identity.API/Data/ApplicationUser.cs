using Microsoft.AspNetCore.Identity;

namespace Identity.API.Data;

/// <summary>System user. The key is a Guid so it can be used directly as the JWT "sub" claim.</summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
}

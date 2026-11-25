using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Clients;
using Web.Models;
using Web.Security;

namespace Web.Controllers;

public sealed class AccountController(IdentityClient identity, ILogger<AccountController> logger) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null, bool expired = false)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        if (expired)
            ViewData["Info"] = "Tu sesión expiró. Inicia sesión de nuevo.";

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl, CancellationToken ct)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
            return View(model);

        LoginResult? result;
        try
        {
            // Synchronous call Web -> Identity
            result = await identity.LoginAsync(model.Email, model.Password, ct);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "No se pudo conectar con Identity");
            ModelState.AddModelError(string.Empty, "No se pudo conectar con el servicio de autenticación. Intenta más tarde.");
            return View(model);
        }

        if (result is null)
        {
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(model);
        }

        var jwt = JwtClaimsReader.Read(result.AccessToken);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            jwt.Claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role));

        // The session ends when the JWT expires
        var properties = new AuthenticationProperties
        {
            ExpiresUtc = jwt.ExpiresAt,
            IsPersistent = false,
            AllowRefresh = false
        };
        properties.StoreTokens([new AuthenticationToken { Name = BearerTokenHandler.TokenName, Value = result.AccessToken }]);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
        logger.LogInformation("Inicio de sesión: {User} ({Roles})",
            principal.Identity?.Name, string.Join(", ", principal.FindAll(ClaimTypes.Role).Select(c => c.Value)));

        // Only local URLs: avoids open redirects to other sites
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}

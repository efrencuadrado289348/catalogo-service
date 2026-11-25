using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Security;

/// <summary>
/// If an API answers 401 (token expired or invalid) the session is closed and the user goes to Login.
/// If it answers 403 the user goes to "Acceso denegado".
/// Typed clients only need response.EnsureSuccessStatusCode() for this to work.
/// </summary>
public sealed class ApiAuthorizationExceptionFilter : IAsyncExceptionFilter
{
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is not HttpRequestException { StatusCode: { } status })
            return;

        if (status == HttpStatusCode.Unauthorized)
        {
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl, expired = true });
            context.ExceptionHandled = true;
        }
        else if (status == HttpStatusCode.Forbidden)
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            context.ExceptionHandled = true;
        }
    }
}

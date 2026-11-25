using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Web.Clients;
using Web.Security;

var builder = WebApplication.CreateBuilder(args);

// Session: cookie that stores the user's claims and the JWT issued by Identity (BFF pattern)
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "ecommerce.session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = false; // the session lasts as long as the JWT
    });

builder.Services.AddAuthorization();

// HttpClients for Identity, Catalog and Orders (Catalog/Orders send the JWT automatically)
builder.Services.AddApiClients(builder.Configuration);
builder.Services.AddHttpClient<IdentityClient>(ApiClients.Identity);
// Typed clients of each feature go here, for example:
builder.Services.AddHttpClient<CatalogClient>(ApiClients.Catalog);
// builder.Services.AddHttpClient<OrdersClient>(ApiClients.Orders);
builder.Services.AddHttpClient<CatalogAdminClient>(ApiClients.Catalog);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter());                       // every page requires login unless [AllowAnonymous]
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()); // CSRF protection on every POST form
    options.Filters.Add<ApiAuthorizationExceptionFilter>();           // API 401 -> login, API 403 -> access denied
});

// Server-side session (in memory), available for the shopping cart
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "ecommerce.cart";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

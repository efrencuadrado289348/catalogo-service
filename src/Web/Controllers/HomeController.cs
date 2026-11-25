using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Clients;
using Web.Models;

namespace Web.Controllers;

public sealed class HomeController(IHttpClientFactory httpClientFactory) : Controller
{
    // Home page: shows that Web talks synchronously with Catalog and Orders using the user's JWT
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var statuses = await Task.WhenAll(
            CheckAsync("Catalog", ApiClients.Catalog, "api/categories", ct),
            CheckAsync("Pedidos", ApiClients.Orders, "api/orders", ct));

        return View(statuses);
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel(Activity.Current?.Id ?? HttpContext.TraceIdentifier));

    private async Task<ServiceStatus> CheckAsync(string service, string clientName, string endpoint, CancellationToken ct)
    {
        try
        {
            var client = httpClientFactory.CreateClient(clientName);
            using var response = await client.GetAsync(endpoint, ct);
            var detail = response.IsSuccessStatusCode ? "Conectado con el token de la sesión" : response.ReasonPhrase ?? "Error";
            return new ServiceStatus(service, endpoint, (int)response.StatusCode, detail);
        }
        catch (HttpRequestException)
        {
            return new ServiceStatus(service, endpoint, null, "Sin conexión");
        }
    }
}

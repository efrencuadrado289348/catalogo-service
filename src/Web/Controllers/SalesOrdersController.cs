using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Clients;
using Web.Security;

namespace Web.Controllers;

[Authorize(Roles = Roles.Vendedor)]
public sealed class SalesOrdersController(OrdersClient orders) : Controller
{
    private static readonly Dictionary<string, int> StatusFilterMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Pending"] = 1,
        ["Confirmed"] = 2,
        ["Shipped"] = 3,
        ["Cancelled"] = 4
    };

    [HttpGet]
    public async Task<IActionResult> Index(string? status, CancellationToken ct)
    {
        var allOrders = (await orders.GetAllOrdersAsync(ct))
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToList();

        if (!string.IsNullOrWhiteSpace(status) && StatusFilterMap.TryGetValue(status, out var statusCode))
        {
            ViewBag.SelectedStatus = status;
            allOrders = allOrders.Where(o => o.Status == statusCode).ToList();
        }
        else
        {
            ViewBag.SelectedStatus = string.Empty;
        }

        return View(allOrders);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(id, ct);
        return order is null ? NotFound() : View(order);
    }

    [HttpPost]
    public async Task<IActionResult> ChangeStatus(Guid id, string status, string? returnUrl, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            TempData["Error"] = "Debes indicar un estado válido.";
            return RedirectBack(returnUrl);
        }

        try
        {
            var updated = await orders.ChangeStatusAsync(id, status, ct);
            TempData["Success"] = $"Pedido actualizado a estado {updated.StatusName}.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectBack(returnUrl);
    }

    private IActionResult RedirectBack(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(nameof(Index));
}

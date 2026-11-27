using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Clients;
using Web.Security;

namespace Web.Controllers;

[Authorize(Roles = Roles.Cliente)]
public sealed class OrdersController(OrdersClient orders) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) =>
        View((await orders.GetMyOrdersAsync(ct)).OrderByDescending(o => o.CreatedAtUtc).ToList());

    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(id, ct);
        return order is null ? NotFound() : View(order);
    }
}

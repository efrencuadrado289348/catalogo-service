using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Cart;
using Web.Clients;
using Web.Security;

namespace Web.Controllers;

[Authorize(Roles = Roles.Cliente)]
public sealed class CartController(CartStore cart, CatalogClient catalog, OrdersClient orders) : Controller
{
    public IActionResult Index() => View(cart.Get());

    [HttpPost]
    public async Task<IActionResult> Add(Guid productId, int quantity, string? returnUrl, CancellationToken ct)
    {
        if (quantity < 1)
            quantity = 1;

        // Nombre y precio se leen de Catalog; nunca se confía en lo que envía el formulario.
        ProductItem? product;
        try
        {
            product = await catalog.GetProductByIdAsync(productId, ct);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            product = null;
        }

        if (product is null || !product.Active || product.Stock < 1)
        {
            TempData["Error"] = "El producto no está disponible.";
            return RedirectBack(returnUrl);
        }

        var lines = cart.Get();
        var existing = lines.FindIndex(l => l.ProductId == product.Id);
        var newQuantity = (existing >= 0 ? lines[existing].Quantity : 0) + quantity;

        if (newQuantity > product.Stock)
        {
            newQuantity = product.Stock;
            TempData["Error"] = $"Solo hay {product.Stock} unidades de \"{product.Name}\".";
        }
        else
        {
            TempData["Success"] = $"\"{product.Name}\" agregado al carrito.";
        }

        var line = new CartLine(product.Id, product.Name, product.Price, product.Currency, newQuantity, product.Stock);
        if (existing >= 0) lines[existing] = line; else lines.Add(line);
        cart.Save(lines);

        return RedirectBack(returnUrl);
    }

    [HttpPost]
    public IActionResult Update(Guid productId, int quantity)
    {
        var lines = cart.Get();
        var index = lines.FindIndex(l => l.ProductId == productId);
        if (index < 0)
            return RedirectToAction(nameof(Index));

        if (quantity < 1)
        {
            lines.RemoveAt(index);
        }
        else
        {
            var max = lines[index].Stock;
            if (quantity > max)
            {
                quantity = max;
                TempData["Error"] = $"Solo hay {max} unidades disponibles.";
            }
            lines[index] = lines[index] with { Quantity = quantity };
        }

        cart.Save(lines);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Remove(Guid productId)
    {
        var lines = cart.Get();
        lines.RemoveAll(l => l.ProductId == productId);
        cart.Save(lines);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Clear()
    {
        cart.Clear();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(CancellationToken ct)
    {
        var lines = cart.Get();
        if (lines.Count == 0)
        {
            TempData["Error"] = "El carrito está vacío.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var order = await orders.CreateAsync(
                new CreateOrder(lines.Select(l => new CreateOrderItem(l.ProductId, l.Quantity)).ToList()), ct);

            cart.Clear();
            TempData["Success"] = "Pedido creado correctamente.";
            return RedirectToAction("Details", "Orders", new { id = order.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    private IActionResult RedirectBack(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction("Index", "Products");
}

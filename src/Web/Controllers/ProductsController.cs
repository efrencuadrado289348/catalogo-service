
using Microsoft.AspNetCore.Mvc;
using Web.Clients;
using Web.Security;

namespace Web.Controllers;

public sealed class ProductsController(CatalogClient catalog) : Controller
{
    public async Task<IActionResult> Index(
        Guid? categoryId,
        CancellationToken ct)
    {
        var products = await catalog.GetProductsAsync(categoryId, ct);
        var categories = await catalog.GetCategoriesAsync(ct);

        ViewBag.Categories = categories;
        ViewBag.SelectedCategoryId = categoryId;
        ViewBag.CanAddToCart = User.IsInRole(Roles.Cliente);

        return View(products);
    }

    public async Task<IActionResult> Details(
        Guid id,
        CancellationToken ct)
    {
        var product = await catalog.GetProductByIdAsync(id, ct);

        if (product is null)
            return NotFound();

        ViewBag.CanAddToCart = User.IsInRole(Roles.Cliente);

        return View(product);
    }
}

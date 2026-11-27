using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Clients;
using Web.Models;
using Web.Security;

namespace Web.Controllers;

[Authorize(Roles = Roles.AdminOrVendedor)]
public sealed class CatalogAdminController(CatalogAdminClient catalog) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var products = await catalog.GetProductsAsync(ct);
        var categories = await catalog.GetCategoriesAsync(ct);
        return View(new CatalogAdminIndexViewModel
        {
            Products = products,
            CategoryNames = categories.ToDictionary(c => c.Id, c => c.Name)
        });
    }

    // ---------- Crear producto (Admin y Vendedor) ----------
    [HttpGet]
    public async Task<IActionResult> CreateProduct(CancellationToken ct) =>
        View(new CreateProductViewModel { Categories = await catalog.GetCategoriesAsync(ct) });

    [HttpPost]
    public async Task<IActionResult> CreateProduct(CreateProductViewModel model, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await catalog.CreateProductAsync(new CreateProductRequest(
                    model.Name, model.Description, model.Price!.Value, model.Currency,
                    model.Stock!.Value, model.CategoryId!.Value), ct);
                TempData["Success"] = $"Producto '{model.Name}' creado.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        model.Categories = await catalog.GetCategoriesAsync(ct);
        return View(model);
    }

    // ---------- Crear categoría (solo Admin) ----------
    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public IActionResult CreateCategory() => View(new CreateCategoryViewModel());

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateCategoryViewModel model, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await catalog.CreateCategoryAsync(new CreateCategoryRequest(model.Name, model.Description), ct);
                TempData["Success"] = $"Categoría '{model.Name}' creada.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }
        return View(model);
    }

    // ---------- Acciones de la tabla ----------
    [HttpPost]
    public async Task<IActionResult> UpdatePrice(Guid id, decimal amount, string currency, CancellationToken ct)
    {
        try
        {
            await catalog.ChangePriceAsync(id, amount, currency, ct);
            TempData["Success"] = "Precio actualizado.";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStock(Guid id, int quantity, CancellationToken ct)
    {
        try
        {
            await catalog.ChangeStockAsync(id, quantity, ct);
            TempData["Success"] = "Stock actualizado.";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        try
        {
            await catalog.DeactivateAsync(id, ct);
            TempData["Success"] = "Producto desactivado.";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }
}

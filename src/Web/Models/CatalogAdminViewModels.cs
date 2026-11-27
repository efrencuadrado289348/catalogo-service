using System.ComponentModel.DataAnnotations;
using Web.Clients;

namespace Web.Models;

public sealed class CatalogAdminIndexViewModel
{
    public IReadOnlyList<AdminProductRow> Products { get; set; } = [];
    public IReadOnlyDictionary<Guid, string> CategoryNames { get; set; } = new Dictionary<Guid, string>();
}

public sealed class CreateCategoryViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class CreateProductViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Sin [Range] a propósito: así el precio negativo llega a la API
    // y se muestra SU mensaje de validación (lo pide la asignación).
    [Required(ErrorMessage = "El precio es obligatorio.")]
    public decimal? Price { get; set; }

    public string Currency { get; set; } = "COP";

    [Required(ErrorMessage = "El stock es obligatorio.")]
    public int? Stock { get; set; }

    [Required(ErrorMessage = "Selecciona una categoría.")]
    public Guid? CategoryId { get; set; }

    public IReadOnlyList<AdminCategoryRow> Categories { get; set; } = [];
}

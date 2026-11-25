using System.Security.Claims;
using System.Text.Json;

namespace Web.Cart;

public sealed record CartLine(Guid ProductId, string Name, decimal UnitPrice, string Currency, int Quantity, int Stock)
{
    public decimal Subtotal => UnitPrice * Quantity;
}

/// <summary>Carrito del usuario en la sesión del servidor. Una clave por usuario (claim "sub").</summary>
public sealed class CartStore(IHttpContextAccessor accessor)
{
    private HttpContext Context => accessor.HttpContext!;
    private string Key => "cart:" + (Context.User.FindFirstValue("sub")
        ?? Context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anon");

    public List<CartLine> Get()
    {
        var json = Context.Session.GetString(Key);
        return string.IsNullOrEmpty(json) ? [] : JsonSerializer.Deserialize<List<CartLine>>(json) ?? [];
    }

    public void Save(List<CartLine> lines) => Context.Session.SetString(Key, JsonSerializer.Serialize(lines));

    public void Clear() => Context.Session.Remove(Key);
}

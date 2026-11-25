using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Web.Clients;

public sealed record OrderItemRow(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal Subtotal);

// Pedidos.API serializa el estado como número (Pending = 1 ... Cancelled = 4).
public sealed record OrderRow(Guid Id, Guid CustomerId, DateTime CreatedAtUtc, int Status, decimal Total,
    IReadOnlyList<OrderItemRow> Items)
{
    public string StatusName => Status switch
    {
        1 => "Pendiente",
        2 => "Confirmado",
        3 => "Enviado",
        4 => "Cancelado",
        _ => "Desconocido"
    };
}

public sealed record CreateOrderItem(Guid ProductId, int Quantity);
public sealed record CreateOrder(List<CreateOrderItem> Items);

public sealed class OrdersClient(HttpClient http)
{
    public async Task<OrderRow> CreateAsync(CreateOrder body, CancellationToken ct)
    {
        using var response = await http.PostAsJsonAsync("api/orders", body, ct);
        await EnsureSuccessAsync(response, ct);
        return (await response.Content.ReadFromJsonAsync<OrderRow>(ct))!;
    }

    public async Task<IReadOnlyList<OrderRow>> GetMyOrdersAsync(CancellationToken ct)
    {
        using var response = await http.GetAsync("api/orders", ct);
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<List<OrderRow>>(ct) ?? [];
    }

    public async Task<OrderRow?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        using var response = await http.GetAsync($"api/orders/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        await EnsureSuccessAsync(response, ct);
        return await response.Content.ReadFromJsonAsync<OrderRow>(ct);
    }

    // 400: Pedidos responde { "message": "..." } (o ProblemDetails). 401/403 los maneja la base.
    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            string? message = null;
            try
            {
                using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
                foreach (var name in new[] { "message", "detail", "title" })
                {
                    if (doc.RootElement.ValueKind == JsonValueKind.Object
                        && doc.RootElement.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String)
                    {
                        message = p.GetString();
                        break;
                    }
                }
            }
            catch (JsonException) { }

            throw new InvalidOperationException(message ?? "Solicitud inválida.");
        }

        response.EnsureSuccessStatusCode();
    }
}

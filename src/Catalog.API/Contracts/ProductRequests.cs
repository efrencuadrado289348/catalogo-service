namespace Catalog.API.Contracts;

public sealed record UpdatePriceRequest(decimal Amount, string? Currency = null);

public sealed record AdjustStockRequest(int Quantity);

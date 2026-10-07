namespace Catalogo.API.Contracts;

public sealed record ActualizarPrecioRequest(decimal Valor, string Moneda);

public sealed record AjustarStockRequest(int Cantidad);

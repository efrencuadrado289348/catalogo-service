using Catalogo.Domain.Exceptions;

namespace Catalogo.Domain.ValueObjects;

/// Value object: no tiene identidad, es inmutable y se compara por valor.
/// Si el valor es inválido, el objeto no se puede crear.
public sealed record Precio
{
    public const string MonedaPorDefecto = "COP";

    public decimal Valor { get; private set; }
    public string Moneda { get; private set; }

    // Constructor privado vacío: lo necesita EF Core para materializar el objeto.
    private Precio()
    {
        Moneda = MonedaPorDefecto;
    }

    private Precio(decimal valor, string moneda)
    {
        Valor = valor;
        Moneda = moneda;
    }

    public static Precio Crear(decimal valor, string? moneda = null)
    {
        if (valor <= 0)
            throw new DomainException("El precio debe ser mayor a cero.");

        var monedaNormalizada = string.IsNullOrWhiteSpace(moneda)
            ? MonedaPorDefecto
            : moneda.Trim().ToUpperInvariant();

        if (monedaNormalizada.Length != 3)
            throw new DomainException("La moneda debe ser un código de 3 letras (por ejemplo, COP o USD).");

        return new Precio(valor, monedaNormalizada);
    }
}

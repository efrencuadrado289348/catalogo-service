namespace Catalogo.Domain.Exceptions;

/// Se lanza cuando se viola una regla de negocio del dominio.
/// La API la convierte en una respuesta 400.
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}

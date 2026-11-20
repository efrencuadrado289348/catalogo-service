namespace Catalog.Domain.Exceptions;

/// <summary>
/// Thrown when a domain business rule is violated.
/// The API converts this into an HTTP 400 Bad Request response.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}

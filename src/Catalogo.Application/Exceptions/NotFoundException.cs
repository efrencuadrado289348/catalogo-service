namespace Catalogo.Application.Exceptions;

/// <summary>Se lanza cuando el recurso solicitado no existe. La API la traduce a HTTP 404.</summary>
public sealed class NotFoundException(string message) : Exception(message);

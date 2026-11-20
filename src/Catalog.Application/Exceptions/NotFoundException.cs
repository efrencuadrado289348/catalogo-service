namespace Catalog.Application.Exceptions;

/// <summary>Thrown when the requested resource is not found. The API translates this to HTTP 404.</summary>
public sealed class NotFoundException(string message) : Exception(message);

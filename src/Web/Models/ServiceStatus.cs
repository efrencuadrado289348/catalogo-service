namespace Web.Models;

/// <summary>Result of calling a protected endpoint of a microservice with the user's token.</summary>
public sealed record ServiceStatus(string Service, string Endpoint, int? StatusCode, string Detail)
{
    public bool Ok => StatusCode is >= 200 and < 300;
}

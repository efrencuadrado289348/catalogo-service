namespace Identity.API.Contracts;

public record LoginResponse(string AccessToken, string TokenType, int ExpiresIn);

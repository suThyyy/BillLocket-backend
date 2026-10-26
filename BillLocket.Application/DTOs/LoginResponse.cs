namespace BillLocket.Application.DTOs;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt,
    string RefreshToken);

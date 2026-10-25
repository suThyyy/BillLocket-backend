namespace BillLocket.Application.DTOs;

public sealed record RegisterResponse(
    string Id,
    string Email,
    string DisplayName,
    DateTimeOffset CreatedAt);

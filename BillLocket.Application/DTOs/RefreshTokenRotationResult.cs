namespace BillLocket.Application.DTOs;

public sealed class RefreshTokenRotationResult
{
    private RefreshTokenRotationResult(
        bool succeeded,
        string? userId,
        string? email,
        string? refreshToken)
    {
        Succeeded = succeeded;
        UserId = userId;
        Email = email;
        RefreshToken = refreshToken;
    }

    public bool Succeeded { get; }

    public string? UserId { get; }

    public string? Email { get; }

    public string? RefreshToken { get; }

    public static RefreshTokenRotationResult Success(
        string userId,
        string email,
        string refreshToken) =>
        new(true, userId, email, refreshToken);

    public static RefreshTokenRotationResult Failure() =>
        new(false, null, null, null);
}

using BillLocket.Application.DTOs;

namespace BillLocket.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<LoginResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ConfirmEmailAsync(
        string userId,
        string token,
        CancellationToken cancellationToken = default);

    Task ResendVerificationAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<LoginResult> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> LogoutAsync(
        string userId,
        LogoutRequest request,
        CancellationToken cancellationToken = default);
}

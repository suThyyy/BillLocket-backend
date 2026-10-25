using BillLocket.Application.DTOs;
using BillLocket.Application.Interfaces;
using BillLocket.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace BillLocket.Infrastructure.Services;

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService) : IAuthService
{
    private const string InvalidCredentialsMessage = "Invalid email or password.";

    public async Task<RegisterResult> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var email = request.Email.Trim();
        var displayName = request.DisplayName.Trim();

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return RegisterResult.Failure(["Display name is required."]);
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName
        };

        // UserManager delegates password hashing to ASP.NET Core Identity's
        // configured password hasher and persists only the resulting hash.
        var identityResult = await userManager.CreateAsync(user, request.Password);

        if (!identityResult.Succeeded)
        {
            return RegisterResult.Failure(
                identityResult.Errors.Select(error => error.Description));
        }

        return RegisterResult.Success(new RegisterResponse(
            user.Id,
            user.Email!,
            user.DisplayName,
            user.CreatedAt));
    }

    public async Task<LoginResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByEmailAsync(request.Email.Trim());

        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return LoginResult.Failure(InvalidCredentialsMessage);
        }

        return LoginResult.Success(
            tokenService.CreateAccessToken(user.Id, user.Email!));
    }
}

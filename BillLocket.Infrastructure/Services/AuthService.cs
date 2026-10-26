using System.Net;
using System.Text;
using BillLocket.Application.DTOs;
using BillLocket.Application.Interfaces;
using BillLocket.Infrastructure.Email;
using BillLocket.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BillLocket.Infrastructure.Services;

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService,
    IEmailSender emailSender,
    IOptions<EmailVerificationOptions> emailVerificationOptions,
    ILogger<AuthService> logger) : IAuthService
{
    private const string InvalidCredentialsMessage = "Invalid email or password.";
    private const string DuplicateEmailMessage = "Email is already registered.";
    private readonly EmailVerificationOptions _emailVerificationOptions =
        ValidateEmailVerificationOptions(emailVerificationOptions.Value);

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
                identityResult.Errors
                    .Select(error => error.Code is "DuplicateEmail" or "DuplicateUserName"
                        ? DuplicateEmailMessage
                        : error.Description)
                    .Distinct());
        }

        await SendVerificationEmailAsync(user, cancellationToken);

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

        if (user is null)
        {
            return LoginResult.Failure(InvalidCredentialsMessage);
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            return LoginResult.Failure(InvalidCredentialsMessage);
        }

        return LoginResult.Success(
            tokenService.CreateAccessToken(user.Id, user.Email!));
    }

    public async Task<bool> ConfirmEmailAsync(
        string userId,
        string token,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return false;
        }

        string decodedToken;

        try
        {
            var tokenBytes = WebEncoders.Base64UrlDecode(token);
            decodedToken = new UTF8Encoding(false, true).GetString(tokenBytes);
        }
        catch (Exception exception) when (
            exception is FormatException or DecoderFallbackException)
        {
            return false;
        }

        var result = await userManager.ConfirmEmailAsync(user, decodedToken);
        return result.Succeeded;
    }

    public async Task ResendVerificationAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await userManager.FindByEmailAsync(email.Trim());

        if (user is null || user.EmailConfirmed)
        {
            return;
        }

        try
        {
            await SendVerificationEmailAsync(user, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to resend verification email for user {UserId}.",
                user.Id);
        }
    }

    private async Task SendVerificationEmailAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var verificationUrl = BuildVerificationUrl(user.Id, encodedToken);
        var encodedUrl = WebUtility.HtmlEncode(verificationUrl.AbsoluteUri);

        var htmlBody = $"""
            <p>Welcome to BillLocket.</p>
            <p>Please verify your email address by clicking the link below:</p>
            <p><a href="{encodedUrl}">Verify email address</a></p>
            <p>If you did not create this account, you can ignore this email.</p>
            """;

        await emailSender.SendEmailAsync(
            user.Email!,
            "Verify your BillLocket email",
            htmlBody,
            cancellationToken);
    }

    private Uri BuildVerificationUrl(string userId, string encodedToken)
    {
        var normalizedBaseUrl = _emailVerificationOptions.PublicBaseUrl.TrimEnd('/') + "/";
        var endpoint = new Uri(new Uri(normalizedBaseUrl), "api/v1/auth/confirm-email");
        var builder = new UriBuilder(endpoint)
        {
            Query = $"userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(encodedToken)}"
        };

        return builder.Uri;
    }

    private static EmailVerificationOptions ValidateEmailVerificationOptions(
        EmailVerificationOptions options)
    {
        options.Validate();
        return options;
    }
}

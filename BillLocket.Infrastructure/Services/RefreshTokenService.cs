
using System.Security.Cryptography;
using System.Text;
using BillLocket.Application.DTOs;
using BillLocket.Application.Interfaces;
using BillLocket.Domain.Entities;
using BillLocket.Infrastructure.Authentication;
using BillLocket.Infrastructure.Identity;
using BillLocket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BillLocket.Infrastructure.Services;

public sealed class RefreshTokenService(
    BillLocketDbContext db,
    UserManager<ApplicationUser> userManager,
    IOptions<RefreshTokenOptions> options) : IRefreshTokenService
{
    private readonly RefreshTokenOptions _options = ValidateOptions(options.Value);

    public async Task<(string RawToken, RefreshToken Entity)> CreateAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var token = CreateToken(userId, Guid.NewGuid(), DateTimeOffset.UtcNow);
        await db.RefreshTokens.AddAsync(token.Entity, cancellationToken);
        return token;
    }

    public async Task<RefreshTokenRotationResult> RotateAsync(
        string rawToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawToken);

        var hash = Hash(rawToken);
        var now = DateTimeOffset.UtcNow;

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var currentToken = await db.RefreshTokens
            .AsNoTracking()
            .SingleOrDefaultAsync(token => token.TokenHash == hash, cancellationToken);

        if (currentToken is null)
        {
            return RefreshTokenRotationResult.Failure();
        }

        if (currentToken.RevokedAt is not null)
        {
            await RevokeActiveFamilyAsync(currentToken.FamilyId, now, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return RefreshTokenRotationResult.Failure();
        }

        if (currentToken.ExpiresAt <= now)
        {
            return RefreshTokenRotationResult.Failure();
        }

        var user = await userManager.FindByIdAsync(currentToken.UserId);

        if (user is null ||
            !user.EmailConfirmed ||
            await userManager.IsLockedOutAsync(user))
        {
            return RefreshTokenRotationResult.Failure();
        }

        var replacement = CreateToken(user.Id, currentToken.FamilyId, now);

        var consumed = await db.RefreshTokens
            .Where(token =>
                token.Id == currentToken.Id &&
                token.RevokedAt == null &&
                token.ExpiresAt > now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(token => token.RevokedAt, now)
                .SetProperty(
                    token => token.ReplacedByTokenId,
                    replacement.Entity.Id),
                cancellationToken);

        if (consumed == 0)
        {
            await RevokeActiveFamilyAsync(currentToken.FamilyId, now, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return RefreshTokenRotationResult.Failure();
        }

        await db.RefreshTokens.AddAsync(replacement.Entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return RefreshTokenRotationResult.Success(
            user.Id,
            user.Email!,
            replacement.RawToken);
    }

    public async Task<bool> RevokeFamilyForUserAsync(
        string rawToken,
        string userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var hash = Hash(rawToken);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var token = await db.RefreshTokens
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.TokenHash == hash, cancellationToken);

        if (token is null || token.UserId != userId)
        {
            return false;
        }

        await RevokeActiveFamilyAsync(token.FamilyId, DateTimeOffset.UtcNow, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);

    private (string RawToken, RefreshToken Entity) CreateToken(
        string userId,
        Guid familyId,
        DateTimeOffset now)
    {
        var rawToken = WebEncoders.Base64UrlEncode(
            RandomNumberGenerator.GetBytes(64));

        var entity = new RefreshToken
        {
            UserId = userId,
            FamilyId = familyId,
            TokenHash = Hash(rawToken),
            CreatedAt = now,
            ExpiresAt = now.AddDays(_options.ExpirationDays)
        };

        return (rawToken, entity);
    }

    private Task<int> RevokeActiveFamilyAsync(
        Guid familyId,
        DateTimeOffset revokedAt,
        CancellationToken cancellationToken) =>
        db.RefreshTokens
            .Where(token => token.FamilyId == familyId && token.RevokedAt == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(token => token.RevokedAt, revokedAt),
                cancellationToken);

    private static string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(rawToken));

        return Convert.ToHexString(bytes);
    }

    private static RefreshTokenOptions ValidateOptions(RefreshTokenOptions options)
    {
        options.Validate();
        return options;
    }
}

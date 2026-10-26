
using BillLocket.Application.DTOs;
using BillLocket.Domain.Entities;

namespace BillLocket.Application.Interfaces;

public interface IRefreshTokenService
{
    Task<(string RawToken, RefreshToken Entity)> CreateAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<RefreshTokenRotationResult> RotateAsync(
        string rawToken,
        CancellationToken cancellationToken = default);

    Task<bool> RevokeFamilyForUserAsync(
        string rawToken,
        string userId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

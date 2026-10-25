using BillLocket.Application.DTOs;

namespace BillLocket.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);
}

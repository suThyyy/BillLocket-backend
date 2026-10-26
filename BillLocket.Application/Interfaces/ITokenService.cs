using BillLocket.Application.DTOs;

namespace BillLocket.Application.Interfaces;

public interface ITokenService
{
    LoginResponse CreateAccessToken(
        string userId,
        string email,
        string refreshToken);
}

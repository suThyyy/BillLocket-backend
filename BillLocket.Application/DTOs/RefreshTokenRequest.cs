
using System.ComponentModel.DataAnnotations;

namespace BillLocket.Application.DTOs;

public sealed class RefreshTokenRequest
{
    [Required]
    [StringLength(512)]
    public string RefreshToken { get; init; } = string.Empty;
}

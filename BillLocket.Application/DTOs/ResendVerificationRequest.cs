using System.ComponentModel.DataAnnotations;

namespace BillLocket.Application.DTOs;

public sealed class ResendVerificationRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace BillLocket.Application.DTOs;

public sealed class RegisterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Display name must not contain only whitespace.")]
    public string DisplayName { get; init; } = string.Empty;

    [Required]
    [StringLength(20, MinimumLength =3)]
    [RegularExpression(
    @"^[a-z0-9_]+$",
    ErrorMessage = "Username must contain only lowercase letters, numbers, and underscores.")]

    public string UserName { get; init; } = string.Empty;
}

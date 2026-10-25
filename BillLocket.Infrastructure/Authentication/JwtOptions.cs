using System.Text;

namespace BillLocket.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public int AccessTokenExpirationMinutes { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException("Jwt:Issuer must be configured.");

        if (string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("Jwt:Audience must be configured.");

        if (Encoding.UTF8.GetByteCount(Key) < 32)
            throw new InvalidOperationException("Jwt:Key must contain at least 32 bytes.");

        if (AccessTokenExpirationMinutes <= 0)
            throw new InvalidOperationException("Jwt:AccessTokenExpirationMinutes must be greater than zero.");
    }
}

namespace BillLocket.Infrastructure.Authentication;

public sealed class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";

    public int ExpirationDays { get; set; } = 7;

    public void Validate()
    {
        if (ExpirationDays is < 1 or > 90)
        {
            throw new InvalidOperationException(
                "RefreshToken:ExpirationDays must be between 1 and 90.");
        }
    }
}

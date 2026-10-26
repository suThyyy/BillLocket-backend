namespace BillLocket.Infrastructure.Email;

public sealed class EmailVerificationOptions
{
    public const string SectionName = "EmailVerification";

    public string PublicBaseUrl { get; set; } = string.Empty;

    public void Validate()
    {
        if (!Uri.TryCreate(PublicBaseUrl, UriKind.Absolute, out var publicBaseUri) ||
            (publicBaseUri.Scheme != Uri.UriSchemeHttp &&
             publicBaseUri.Scheme != Uri.UriSchemeHttps) ||
            (publicBaseUri.Scheme == Uri.UriSchemeHttp && !publicBaseUri.IsLoopback) ||
            !string.IsNullOrEmpty(publicBaseUri.UserInfo) ||
            !string.IsNullOrEmpty(publicBaseUri.Query) ||
            !string.IsNullOrEmpty(publicBaseUri.Fragment))
        {
            throw new InvalidOperationException(
                "EmailVerification:PublicBaseUrl must be a trusted absolute HTTPS URL, or HTTP loopback URL, without credentials, query, or fragment.");
        }
    }
}

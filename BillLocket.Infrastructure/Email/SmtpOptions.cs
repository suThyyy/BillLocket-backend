using MailKit.Security;
using MimeKit;

namespace BillLocket.Infrastructure.Email;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public SecureSocketOptions SocketOptions { get; set; } = SecureSocketOptions.StartTls;

    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } = string.Empty;

    public string? Username { get; set; }

    public string? Password { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Host))
            throw new InvalidOperationException("Smtp:Host must be configured.");

        if (Port is < 1 or > 65535)
            throw new InvalidOperationException("Smtp:Port must be between 1 and 65535.");

        if (!MailboxAddress.TryParse(FromEmail, out var fromAddress) ||
            !string.Equals(fromAddress.Address, FromEmail.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Smtp:FromEmail must be a valid email address.");

        if (string.IsNullOrWhiteSpace(FromName))
            throw new InvalidOperationException("Smtp:FromName must be configured.");

        if (string.IsNullOrWhiteSpace(Username) != string.IsNullOrWhiteSpace(Password))
            throw new InvalidOperationException("Smtp:Username and Smtp:Password must be configured together.");

        if (!string.IsNullOrWhiteSpace(Username) &&
            SocketOptions is not SecureSocketOptions.StartTls and
                not SecureSocketOptions.SslOnConnect)
        {
            throw new InvalidOperationException(
                "Authenticated SMTP requires Smtp:SocketOptions to be StartTls or SslOnConnect.");
        }
    }
}

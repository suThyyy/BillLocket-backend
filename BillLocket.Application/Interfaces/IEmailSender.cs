namespace BillLocket.Application.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}

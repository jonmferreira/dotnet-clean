namespace Parking.Application.Abstractions;

public interface IEmailSender
{
    Task SendEmailAsync(
        string to,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken cancellationToken = default);
}

using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parking.Application.Abstractions;

namespace Parking.Infrastructure.Email;

public sealed class AwsSesEmailSender : IEmailSender, IDisposable
{
    private readonly AwsSesOptions _options;
    private readonly ILogger<AwsSesEmailSender> _logger;
    private readonly Lazy<AmazonSimpleEmailServiceV2Client> _clientFactory;
    private bool _disposed;

    public AwsSesEmailSender(IOptions<AwsSesOptions> options, ILogger<AwsSesEmailSender> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrWhiteSpace(_options.Region))
        {
            throw new ArgumentException("AWS region must be provided.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(_options.FromAddress))
        {
            throw new ArgumentException("Sender address must be provided.", nameof(options));
        }

        if (!string.IsNullOrWhiteSpace(_options.AccessKeyId) && string.IsNullOrWhiteSpace(_options.SecretAccessKey))
        {
            throw new ArgumentException("Secret access key must be provided when using explicit AWS credentials.", nameof(options));
        }

        if (!string.IsNullOrWhiteSpace(_options.SecretAccessKey) && string.IsNullOrWhiteSpace(_options.AccessKeyId))
        {
            throw new ArgumentException("Access key id must be provided when using explicit AWS credentials.", nameof(options));
        }

        _clientFactory = new Lazy<AmazonSimpleEmailServiceV2Client>(CreateClient, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public async Task SendEmailAsync(
        string to,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            throw new ArgumentException("Recipient must be provided.", nameof(to));
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Subject must be provided.", nameof(subject));
        }

        if (string.IsNullOrWhiteSpace(htmlBody))
        {
            throw new ArgumentException("Email body must be provided.", nameof(htmlBody));
        }

        var sendRequest = new SendEmailRequest
        {
            FromEmailAddress = _options.FromAddress,
            Destination = new Destination
            {
                ToAddresses = new List<string> { to }
            },
            Content = new EmailContent
            {
                Simple = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Html = new Content(htmlBody),
                        Text = string.IsNullOrWhiteSpace(textBody) ? null : new Content(textBody)
                    }
                }
            }
        };

        if (!string.IsNullOrWhiteSpace(_options.ConfigurationSetName))
        {
            sendRequest.ConfigurationSetName = _options.ConfigurationSetName;
        }

        try
        {
            var response = await _clientFactory.Value.SendEmailAsync(sendRequest, cancellationToken);
            _logger.LogInformation("AWS SES message sent with id {MessageId} and status code {StatusCode}.",
                response.MessageId, response.HttpStatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via AWS SES.");
            throw;
        }
    }

    private AmazonSimpleEmailServiceV2Client CreateClient()
    {
        var region = RegionEndpoint.GetBySystemName(_options.Region);
        if (!string.IsNullOrWhiteSpace(_options.AccessKeyId) && !string.IsNullOrWhiteSpace(_options.SecretAccessKey))
        {
            var credentials = new BasicAWSCredentials(_options.AccessKeyId, _options.SecretAccessKey);
            return new AmazonSimpleEmailServiceV2Client(credentials, region);
        }

        return new AmazonSimpleEmailServiceV2Client(region);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_clientFactory.IsValueCreated)
        {
            _clientFactory.Value.Dispose();
        }

        _disposed = true;
    }
}

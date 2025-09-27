using System.Collections.Generic;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parking.Application.Abstractions;

namespace Parking.Infrastructure.Messaging;

public sealed class AwsSmsSender : ISmsSender
{
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly AwsSmsOptions _options;
    private readonly ILogger<AwsSmsSender> _logger;

    public AwsSmsSender(
        IAmazonSimpleNotificationService snsClient,
        IOptions<AwsSmsOptions> options,
        ILogger<AwsSmsSender> logger)
    {
        _snsClient = snsClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var request = new PublishRequest
        {
            PhoneNumber = phoneNumber,
            Message = message,
        };

        var attributes = request.MessageAttributes ??= new Dictionary<string, MessageAttributeValue>();

        if (!string.IsNullOrWhiteSpace(_options.SenderId))
        {
            attributes["AWS.SNS.SMS.SenderID"] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = _options.SenderId
            };
        }

        if (!string.IsNullOrWhiteSpace(_options.SmsType))
        {
            attributes["AWS.SNS.SMS.SMSType"] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = _options.SmsType
            };
        }

        if (!string.IsNullOrWhiteSpace(_options.MaxPrice))
        {
            attributes["AWS.SNS.SMS.MaxPrice"] = new MessageAttributeValue
            {
                DataType = "Number",
                StringValue = _options.MaxPrice
            };
        }

        try
        {
            await _snsClient.PublishAsync(request, cancellationToken);
        }
        catch (AmazonSimpleNotificationServiceException ex)
        {
            _logger.LogError(ex, "Failed to send SMS via AWS SNS.");
            throw;
        }
    }
}

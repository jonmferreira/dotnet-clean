namespace Parking.Infrastructure.Messaging;

public sealed record AwsSmsOptions
{
    public const string SectionName = "Aws:Sms";

    public required string Region { get; init; }

    public string? AccessKey { get; init; }

    public string? SecretKey { get; init; }

    public string? SenderId { get; init; }

    public string? SmsType { get; init; }

    public string? MaxPrice { get; init; }
}

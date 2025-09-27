namespace Parking.Infrastructure.Email;

public sealed class AwsSesOptions
{
    public string AccessKeyId { get; set; } = string.Empty;

    public string SecretAccessKey { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string FromAddress { get; set; } = string.Empty;

    public string? ConfigurationSetName { get; set; }
}

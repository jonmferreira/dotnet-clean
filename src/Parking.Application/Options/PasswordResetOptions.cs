namespace Parking.Application.Options;

public sealed class PasswordResetOptions
{
    public int TokenExpirationMinutes { get; set; } = 60;

    public string ResetUrl { get; set; } = string.Empty;
}

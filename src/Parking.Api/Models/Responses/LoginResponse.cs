namespace Parking.Api.Models.Responses;

public sealed class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public AuthenticatedUserResponse User { get; set; } = new();
}

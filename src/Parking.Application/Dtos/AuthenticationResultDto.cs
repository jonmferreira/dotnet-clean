namespace Parking.Application.Dtos;

public sealed record AuthenticationResultDto(
    Guid UserId,
    string Name,
    string Email,
    string Role,
    string AccessToken,
    DateTimeOffset ExpiresAt);

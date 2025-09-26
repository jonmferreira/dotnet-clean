using Parking.Domain.Entities;

namespace Parking.Application.Abstractions.Security;

public interface IJwtTokenGenerator
{
    JwtTokenResult GenerateToken(User user);
}

public sealed record JwtTokenResult(string Token, DateTimeOffset ExpiresAt);

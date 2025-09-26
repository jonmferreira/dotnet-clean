using System;
using System.Collections.Generic;

namespace Parking.Application.Authentication;

public interface IJwtTokenGenerator
{
    JwtTokenResult GenerateToken(
        Guid userId,
        string email,
        IReadOnlyCollection<string>? roles = null,
        TimeSpan? lifetime = null);
}

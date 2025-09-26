using System;
using System.Collections.Generic;

namespace Parking.Application.Authentication;

public sealed record JwtTokenResult(
    string Token,
    DateTimeOffset ExpiresAt,
    Guid UserId,
    string Email,
    IReadOnlyCollection<string> Roles);

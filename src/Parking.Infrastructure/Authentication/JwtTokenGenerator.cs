using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Parking.Application.Authentication;

namespace Parking.Infrastructure.Authentication;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private static readonly TimeSpan DefaultLifetime = TimeSpan.FromHours(1);
    private readonly TimeProvider _timeProvider;

    public JwtTokenGenerator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public JwtTokenResult GenerateToken(
        Guid userId,
        string email,
        IReadOnlyCollection<string>? roles = null,
        TimeSpan? lifetime = null)
    {
        ArgumentNullException.ThrowIfNull(email);

        var normalizedEmail = email.Trim();
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            throw new ArgumentException("Email must be provided.", nameof(email));
        }

        var sanitizedRoles = roles?
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? Array.Empty<string>();

        var expiresAt = _timeProvider.GetUtcNow().Add(lifetime ?? DefaultLifetime);

        using var rng = RandomNumberGenerator.Create();
        var secret = new byte[32];
        rng.GetBytes(secret);

        var payload = string.Join(
            '|',
            userId.ToString("N"),
            normalizedEmail,
            expiresAt.ToUnixTimeSeconds().ToString());

        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(secret);
        var signature = hmac.ComputeHash(payloadBytes);

        var token = string.Join(
            '.',
            Convert.ToBase64String(payloadBytes),
            Convert.ToBase64String(signature));

        return new JwtTokenResult(token, expiresAt, userId, normalizedEmail, sanitizedRoles);
    }
}

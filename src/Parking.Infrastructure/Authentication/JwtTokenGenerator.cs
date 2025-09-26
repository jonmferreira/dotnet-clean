using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Parking.Application.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Parking.Application.Abstractions.Security;
using Parking.Domain.Entities;


namespace Parking.Infrastructure.Authentication;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{

    private static readonly TimeSpan DefaultLifetime = TimeSpan.FromHours(1);
    private readonly TimeProvider _timeProvider;
    private readonly JwtOptions _options;

    public JwtTokenGenerator(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        ValidateOptions(_options);
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



    public JwtTokenResult GenerateToken(User user)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var expiration = DateTimeOffset.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiration.UtcDateTime,
            signingCredentials: signingCredentials);

        var tokenHandler = new JwtSecurityTokenHandler();
        var accessToken = tokenHandler.WriteToken(token);

        return new JwtTokenResult(accessToken, expiration);
    }

    private static void ValidateOptions(JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.SecretKey) || options.SecretKey.Length < 16)
        {
            throw new InvalidOperationException("JWT secret key must be provided and contain at least 16 characters.");
        }

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            throw new InvalidOperationException("JWT issuer must be provided.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            throw new InvalidOperationException("JWT audience must be provided.");
        }

        if (options.AccessTokenExpirationMinutes <= 0)
        {
            throw new InvalidOperationException("JWT expiration must be greater than zero.");
        }

    }
}

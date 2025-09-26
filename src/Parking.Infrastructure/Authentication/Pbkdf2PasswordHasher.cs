using System;
using System.Globalization;
using System.Security.Cryptography;
using Parking.Application.Authentication;

namespace Parking.Infrastructure.Authentication;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32;  // 256 bits
    private const int DefaultIterations = 100_000;

    public string HashPassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            DefaultIterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return string.Join(
            '.',
            DefaultIterations.ToString(CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        ArgumentNullException.ThrowIfNull(hashedPassword);
        ArgumentNullException.ThrowIfNull(providedPassword);

        var segments = hashedPassword.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length != 3
            || !int.TryParse(segments[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(segments[1]);
        var expected = Convert.FromBase64String(segments[2]);

        var actual = Rfc2898DeriveBytes.Pbkdf2(
            providedPassword,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expected.Length);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}

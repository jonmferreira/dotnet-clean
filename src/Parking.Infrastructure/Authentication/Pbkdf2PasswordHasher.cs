using System;
using System.Globalization;
using System.Security.Cryptography;
using Parking.Application.Authentication;
using Parking.Application.Abstractions.Security;


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

    
    private const int KeySize = 32; // 256 bits
    private const int Iterations = 100_000;

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Password must not be null or empty.", nameof(password));
        }

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

        return string.Join('.', Iterations.ToString(), Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword))
        {
            return false;
        }

        var parts = hashedPassword.Split('.');
        if (parts.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out var iterations))
        {
            return false;
        }


        var salt = Convert.FromBase64String(segments[1]);
        var expected = Convert.FromBase64String(segments[2]);

        var actual = Rfc2898DeriveBytes.Pbkdf2(

        var salt = Convert.FromBase64String(parts[1]);
        var hash = Convert.FromBase64String(parts[2]);

        var computedHash = Rfc2898DeriveBytes.Pbkdf2(
            providedPassword,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expected.Length);

 
        return CryptographicOperations.FixedTimeEquals(hash, computedHash);
    }
}
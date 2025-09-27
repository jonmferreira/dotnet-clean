using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parking.Application.Abstractions;
using Parking.Application.Abstractions.Security;
using Parking.Application.Options;
using Parking.Domain.Entities;
using Parking.Domain.Repositories;

namespace Parking.Application.Services;

public sealed class PasswordResetService : IPasswordResetService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _tokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _emailSender;
    private readonly PasswordResetOptions _options;
    private readonly ILogger<PasswordResetService> _logger;

    public PasswordResetService(
        IUserRepository userRepository,
        IPasswordResetTokenRepository tokenRepository,
        IPasswordHasher passwordHasher,
        IEmailSender emailSender,
        IOptions<PasswordResetOptions> options,
        ILogger<PasswordResetService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _tokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (_options.TokenExpirationMinutes <= 0)
        {
            throw new ArgumentException("Token expiration must be greater than zero.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(_options.ResetUrl))
        {
            throw new ArgumentException("Reset URL must be configured.", nameof(options));
        }
    }

    public async Task RequestResetAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email must be provided.", nameof(email));
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            _logger.LogInformation("Password reset requested for unknown email {Email}.", normalizedEmail);
            return;
        }

        var tokenValue = GenerateToken();
        var tokenHash = HashToken(tokenValue);

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.TokenExpirationMinutes);

        var token = new PasswordResetToken(Guid.NewGuid(), user.Id, tokenHash, expiresAt, DateTimeOffset.UtcNow);
        await _tokenRepository.AddAsync(token, cancellationToken);

        var resetLink = BuildResetLink(tokenValue);

        var subject = "Recuperação de senha";
        var htmlBody = $"<p>Olá, {user.Name}!</p>" +
                       "<p>Recebemos um pedido para redefinir a sua senha. Clique no botão abaixo para continuar:</p>" +
                       $"<p><a href=\"{resetLink}\" style=\"background-color:#2563eb;color:#fff;padding:10px 16px;text-decoration:none;border-radius:6px;\">Redefinir senha</a></p>" +
                       $"<p>Se o botão não funcionar, copie e cole este endereço no seu navegador:<br /><a href=\"{resetLink}\">{resetLink}</a></p>" +
                       $"<p>Este link expira em {_options.TokenExpirationMinutes} minutos. Se você não solicitou a alteração, ignore este e-mail.</p>";

        var textBody = $"Olá, {user.Name}!\\n\\n" +
                       "Recebemos um pedido para redefinir a sua senha. Use o link abaixo:\\n" +
                       $"{resetLink}\\n\\n" +
                       $"O link expira em {_options.TokenExpirationMinutes} minutos. Se você não solicitou a alteração, ignore esta mensagem.";
        await _emailSender.SendEmailAsync(user.Email, subject, htmlBody, textBody, cancellationToken);
    }

    public async Task ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token must be provided.", nameof(token));
        }

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            throw new ArgumentException("Password must be provided.", nameof(newPassword));
        }

        var normalizedPassword = newPassword.Trim();

        if (normalizedPassword.Length < 6)
        {
            throw new ArgumentException("Password must contain at least six characters.", nameof(newPassword));
        }

        var tokenHash = HashToken(token);
        var passwordResetToken = await _tokenRepository.GetByHashAsync(tokenHash, cancellationToken);

        if (passwordResetToken is null)
        {
            throw new InvalidOperationException("Invalid or expired token.");
        }

        if (passwordResetToken.IsUsed || passwordResetToken.IsExpired)
        {
            throw new InvalidOperationException("Invalid or expired token.");
        }

        var user = await _userRepository.GetByIdAsync(passwordResetToken.UserId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("User not found for the provided token.");
        }

        var hashedPassword = _passwordHasher.HashPassword(normalizedPassword);
        user.UpdatePasswordHash(hashedPassword);
        await _userRepository.UpdateAsync(user, cancellationToken);

        passwordResetToken.MarkAsUsed();
        await _tokenRepository.UpdateAsync(passwordResetToken, cancellationToken);
    }

    private static string GenerateToken()
    {
        Span<byte> buffer = stackalloc byte[32];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToBase64String(buffer)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    private static string HashToken(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToHexString(hashBytes);
    }

    private string BuildResetLink(string token)
    {
        return _options.ResetUrl.Contains("?", StringComparison.Ordinal)
            ? $"{_options.ResetUrl}&token={token}"
            : $"{_options.ResetUrl}?token={token}";
    }
}

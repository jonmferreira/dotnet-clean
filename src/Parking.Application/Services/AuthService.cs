using Parking.Application.Abstractions;
using Parking.Application.Abstractions.Security;
using Parking.Application.Dtos;
using Parking.Domain.Repositories;

namespace Parking.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
    }

    public async Task<AuthenticationResultDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Email and password must be provided.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var isValidPassword = _passwordHasher.VerifyHashedPassword(user.PasswordHash, request.Password);

        if (!isValidPassword)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthenticationResultDto(user.Id, user.Name, user.Email, user.Role, token.Token, token.ExpiresAt);
    }
}

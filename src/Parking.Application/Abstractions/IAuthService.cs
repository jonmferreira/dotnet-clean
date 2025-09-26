using Parking.Application.Dtos;

namespace Parking.Application.Abstractions;

public interface IAuthService
{
    Task<AuthenticationResultDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
}

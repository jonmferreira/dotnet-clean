namespace Parking.Application.Abstractions;

public interface IPasswordResetService
{
    Task RequestResetAsync(string email, CancellationToken cancellationToken = default);

    Task ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default);
}

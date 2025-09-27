using Parking.Application.Dtos;

namespace Parking.Application.Abstractions;

public interface ICepLookupService
{
    Task<CepAddressDto?> GetAddressByCepAsync(string cep, CancellationToken cancellationToken = default);
}

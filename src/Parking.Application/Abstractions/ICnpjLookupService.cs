using Parking.Application.Dtos.Cnpj;

namespace Parking.Application.Abstractions;

public interface ICnpjLookupService
{
    Task<CnpjCompanyDto?> GetCompanyAsync(string cnpj, CancellationToken cancellationToken = default);
}

using System.Globalization;
using Parking.Api.Models.Responses;
using Parking.Application.Dtos.Cnpj;

namespace Parking.Api.Mappings;

public static class CnpjMappingExtensions
{
    public static CnpjCompanyResponse ToResponse(this CnpjCompanyDto dto)
    {
        return new CnpjCompanyResponse(
            dto.Cnpj,
            dto.CorporateName,
            dto.TradeName,
            dto.Status,
            dto.MainActivity,
            dto.Email,
            dto.Phone,
            dto.FoundedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            dto.Address.ToResponse());
    }

    public static CnpjCompanyAddressResponse? ToResponse(this CnpjCompanyAddressDto? dto)
    {
        if (dto is null)
        {
            return null;
        }

        return new CnpjCompanyAddressResponse(
            dto.Street,
            dto.Number,
            dto.Complement,
            dto.Neighborhood,
            dto.City,
            dto.State,
            dto.ZipCode);
    }
}

using System;

namespace Parking.Application.Dtos.Cnpj;

public sealed record CnpjCompanyDto(
    string Cnpj,
    string? CorporateName,
    string? TradeName,
    string? Status,
    string? MainActivity,
    string? Email,
    string? Phone,
    DateOnly? FoundedAt,
    CnpjCompanyAddressDto? Address);

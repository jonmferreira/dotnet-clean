namespace Parking.Api.Models.Responses;

public sealed record CnpjCompanyResponse(
    string Cnpj,
    string? CorporateName,
    string? TradeName,
    string? Status,
    string? MainActivity,
    string? Email,
    string? Phone,
    string? FoundedAt,
    CnpjCompanyAddressResponse? Address);

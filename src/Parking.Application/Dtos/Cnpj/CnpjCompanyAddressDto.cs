namespace Parking.Application.Dtos.Cnpj;

public sealed record CnpjCompanyAddressDto(
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State,
    string? ZipCode);

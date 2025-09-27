namespace Parking.Api.Models.Responses;

public sealed record CnpjCompanyAddressResponse(
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State,
    string? ZipCode);

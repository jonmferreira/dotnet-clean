namespace Parking.Application.Dtos;

public sealed record CepAddressDto(
    string Cep,
    string Street,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string? Ibge,
    string? Gia,
    string? Ddd,
    string? Siafi);

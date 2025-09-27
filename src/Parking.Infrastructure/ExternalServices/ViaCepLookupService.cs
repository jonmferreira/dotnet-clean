using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Parking.Application.Abstractions;
using Parking.Application.Dtos;

namespace Parking.Infrastructure.ExternalServices;

public sealed class ViaCepLookupService : ICepLookupService
{
    private readonly HttpClient _httpClient;

    public ViaCepLookupService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<CepAddressDto?> GetAddressByCepAsync(string cep, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cep))
        {
            throw new ArgumentException("CEP must be provided.", nameof(cep));
        }

        var sanitizedCep = SanitizeCep(cep);
        if (sanitizedCep.Length != 8)
        {
            throw new ArgumentException("CEP must contain exactly 8 digits.", nameof(cep));
        }

        using var response = await _httpClient.GetAsync($"ws/{sanitizedCep}/json/", cancellationToken);

        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var viaCepResponse = await response.Content.ReadFromJsonAsync<ViaCepResponse>(cancellationToken: cancellationToken);

        if (viaCepResponse is null || viaCepResponse.Erro)
        {
            return null;
        }

        return new CepAddressDto(
            viaCepResponse.Cep ?? sanitizedCep,
            viaCepResponse.Logradouro ?? string.Empty,
            viaCepResponse.Complemento,
            viaCepResponse.Bairro ?? string.Empty,
            viaCepResponse.Localidade ?? string.Empty,
            viaCepResponse.Uf ?? string.Empty,
            viaCepResponse.Ibge,
            viaCepResponse.Gia,
            viaCepResponse.Ddd,
            viaCepResponse.Siafi);
    }

    private static string SanitizeCep(string cep)
    {
        Span<char> buffer = stackalloc char[cep.Length];
        var count = 0;

        foreach (var ch in cep)
        {
            if (char.IsDigit(ch))
            {
                buffer[count++] = ch;
            }
        }

        return new string(buffer[..count]);
    }

    private sealed record ViaCepResponse
    {
        [JsonPropertyName("cep")]
        public string? Cep { get; init; }

        [JsonPropertyName("logradouro")]
        public string? Logradouro { get; init; }

        [JsonPropertyName("complemento")]
        public string? Complemento { get; init; }

        [JsonPropertyName("bairro")]
        public string? Bairro { get; init; }

        [JsonPropertyName("localidade")]
        public string? Localidade { get; init; }

        [JsonPropertyName("uf")]
        public string? Uf { get; init; }

        [JsonPropertyName("ibge")]
        public string? Ibge { get; init; }

        [JsonPropertyName("gia")]
        public string? Gia { get; init; }

        [JsonPropertyName("ddd")]
        public string? Ddd { get; init; }

        [JsonPropertyName("siafi")]
        public string? Siafi { get; init; }

        [JsonPropertyName("erro")]
        public bool Erro { get; init; }
    }
}

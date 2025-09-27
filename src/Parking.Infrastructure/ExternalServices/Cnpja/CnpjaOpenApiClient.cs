using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parking.Application.Abstractions;
using Parking.Application.Dtos.Cnpj;

namespace Parking.Infrastructure.ExternalServices.Cnpja;

internal sealed class CnpjaOpenApiClient : ICnpjLookupService
{
    private readonly HttpClient _httpClient;
    private readonly IOptionsMonitor<CnpjaOptions> _options;
    private readonly ILogger<CnpjaOpenApiClient> _logger;

    public CnpjaOpenApiClient(
        HttpClient httpClient,
        IOptionsMonitor<CnpjaOptions> options,
        ILogger<CnpjaOpenApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<CnpjCompanyDto?> GetCompanyAsync(string cnpj, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
        {
            throw new ArgumentException("CNPJ must be provided.", nameof(cnpj));
        }

        var sanitizedCnpj = new string(cnpj.Where(char.IsDigit).ToArray());
        if (sanitizedCnpj.Length != 14)
        {
            throw new ArgumentException("CNPJ must contain 14 digits.", nameof(cnpj));
        }

        var options = _options.CurrentValue;
        var requestPath = BuildCompanyEndpoint(options.CompanyEndpoint, sanitizedCnpj);

        using var request = new HttpRequestMessage(HttpMethod.Get, requestPath);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrWhiteSpace(options.Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Token);
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "CNPJa lookup failed with status code {StatusCode}. Response: {Response}",
                (int)response.StatusCode,
                errorBody);

            throw new HttpRequestException(
                $"CNPJa API returned status code {(int)response.StatusCode}: {errorBody}");
        }

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken);

        return MapCompany(document.RootElement);
    }

    private static string BuildCompanyEndpoint(string? template, string cnpj)
    {
        var normalizedTemplate = string.IsNullOrWhiteSpace(template)
            ? CnpjaOptions.DefaultCompanyEndpoint
            : template.Trim();

        var path = normalizedTemplate.Contains("{0}", StringComparison.Ordinal)
            ? string.Format(CultureInfo.InvariantCulture, normalizedTemplate, cnpj)
            : string.Concat(normalizedTemplate.TrimEnd('/'), "/", cnpj);

        return path.TrimStart('/');
    }

    private static CnpjCompanyDto MapCompany(JsonElement root)
    {
        var cnpjValue = GetString(root, "taxId", "cnpj", "id", "document")
            ?? throw new JsonException("The CNPJa response did not include a CNPJ identifier.");

        var normalizedCnpj = new string(cnpjValue.Where(char.IsDigit).ToArray());
        if (normalizedCnpj.Length == 14)
        {
            cnpjValue = normalizedCnpj;
        }

        var corporateName = GetString(root, "name", "razao_social", "corporateName", "businessName");
        var tradeName = GetString(root, "alias", "nome_fantasia", "tradeName", "fantasyName");
        var status = GetString(root, "status", "situacao");
        var foundedAt = GetDate(root, "founded", "foundation", "openingDate", "abertura");
        var mainActivity = GetMainActivity(root);
        var email = GetString(root, "email", "emails");
        var phone = GetString(root, "phone", "phones");
        var address = MapAddress(root);

        return new CnpjCompanyDto(
            cnpjValue,
            corporateName,
            tradeName,
            status,
            mainActivity,
            email,
            phone,
            foundedAt,
            address);
    }

    private static CnpjCompanyAddressDto? MapAddress(JsonElement root)
    {
        if (!TryGetProperty(root, out var addressElement, "address", "endereco"))
        {
            return null;
        }

        if (addressElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return new CnpjCompanyAddressDto(
            GetString(addressElement, "street", "logradouro", "addressLine"),
            GetString(addressElement, "number", "numero"),
            GetString(addressElement, "complement", "complemento"),
            GetString(addressElement, "district", "bairro"),
            GetString(addressElement, "city", "municipio", "cityName"),
            GetString(addressElement, "state", "uf", "stateCode"),
            GetString(addressElement, "zip", "cep", "postalCode"));
    }

    private static string? GetMainActivity(JsonElement root)
    {
        if (TryGetProperty(root, out var directActivity, "mainActivity", "primaryActivity", "atividade_principal"))
        {
            var value = ConvertToString(directActivity);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        if (TryGetProperty(root, out var activities, "activities", "atividades"))
        {
            if (activities.ValueKind == JsonValueKind.Array)
            {
                foreach (var activity in activities.EnumerateArray())
                {
                    if (activity.ValueKind != JsonValueKind.Object)
                    {
                        var candidate = ConvertToString(activity);
                        if (!string.IsNullOrWhiteSpace(candidate))
                        {
                            return candidate;
                        }

                        continue;
                    }

                    if (activity.TryGetProperty("isMain", out var isMainElement)
                        && isMainElement.ValueKind == JsonValueKind.True)
                    {
                        var candidate = GetString(activity, "text", "description", "name");
                        if (!string.IsNullOrWhiteSpace(candidate))
                        {
                            return candidate;
                        }
                    }

                    var fallback = GetString(activity, "text", "description", "name");
                    if (!string.IsNullOrWhiteSpace(fallback))
                    {
                        return fallback;
                    }
                }
            }
            else
            {
                var candidate = ConvertToString(activities);
                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    return candidate;
                }
            }
        }

        if (TryGetProperty(root, out var activityObject, "activity"))
        {
            var candidate = GetString(activityObject, "text", "description", "name");
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static DateOnly? GetDate(JsonElement element, params string[] propertyNames)
    {
        var text = GetString(element, propertyNames);
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        text = text.Trim();
        if (DateOnly.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnly))
        {
            return dateOnly;
        }

        var brazilCulture = CultureInfo.GetCultureInfo("pt-BR");
        if (DateOnly.TryParse(text, brazilCulture, DateTimeStyles.None, out dateOnly))
        {
            return dateOnly;
        }

        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
        {
            return DateOnly.FromDateTime(dateTime);
        }

        if (DateTime.TryParse(text, brazilCulture, DateTimeStyles.None, out dateTime))
        {
            return DateOnly.FromDateTime(dateTime);
        }

        return null;
    }

    private static string? GetString(JsonElement element, params string[] propertyNames)
    {
        if (!TryGetProperty(element, out var property, propertyNames))
        {
            return null;
        }

        return ConvertToString(property);
    }

    private static string? ConvertToString(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return element.GetString()?.Trim();
            case JsonValueKind.Number:
                return element.GetRawText();
            case JsonValueKind.True:
            case JsonValueKind.False:
                return element.GetBoolean().ToString();
            case JsonValueKind.Object:
                return GetString(element, "text", "description", "value", "name");
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    var candidate = ConvertToString(item);
                    if (!string.IsNullOrWhiteSpace(candidate))
                    {
                        return candidate;
                    }
                }

                break;
        }

        return null;
    }

    private static bool TryGetProperty(JsonElement element, out JsonElement property, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out property))
            {
                return true;
            }
        }

        property = default;
        return false;
    }
}

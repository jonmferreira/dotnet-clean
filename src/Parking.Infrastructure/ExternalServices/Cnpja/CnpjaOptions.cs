namespace Parking.Infrastructure.ExternalServices.Cnpja;

public sealed class CnpjaOptions
{
    public const string SectionName = "Cnpja";
    public const string DefaultBaseUrl = "https://api.cnpja.com.br/";
    public const string DefaultCompanyEndpoint = "companies/{0}";

    public string BaseUrl { get; set; } = DefaultBaseUrl;

    public string CompanyEndpoint { get; set; } = DefaultCompanyEndpoint;

    public string? Token { get; set; }
}

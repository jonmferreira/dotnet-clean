namespace Parking.Api.Models.Responses;

public sealed record DashboardChartPointResponse
{
    public string Name { get; init; } = string.Empty;

    public int Value { get; init; }
}

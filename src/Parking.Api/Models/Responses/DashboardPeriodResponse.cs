namespace Parking.Api.Models.Responses;

public sealed record DashboardPeriodResponse
{
    public string Name { get; init; } = string.Empty;

    public int Entries { get; init; }
}

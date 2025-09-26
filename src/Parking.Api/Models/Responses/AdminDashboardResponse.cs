namespace Parking.Api.Models.Responses;

public sealed record AdminDashboardResponse
{
    public IReadOnlyCollection<DashboardChartPointResponse> WeeklyEntries { get; init; } = Array.Empty<DashboardChartPointResponse>();

    public IReadOnlyCollection<DashboardChartPointResponse> EntriesByDayOfWeek { get; init; } = Array.Empty<DashboardChartPointResponse>();

    public IReadOnlyCollection<DashboardChartPointResponse> MonthlyEntries { get; init; } = Array.Empty<DashboardChartPointResponse>();

    public IReadOnlyCollection<DashboardChartPointResponse> YearlyEntries { get; init; } = Array.Empty<DashboardChartPointResponse>();

    public IReadOnlyCollection<DashboardPeriodResponse> TrafficByPeriod { get; init; } = Array.Empty<DashboardPeriodResponse>();

    public MonthlyTargetResponse MonthlyTarget { get; init; } = new();
}

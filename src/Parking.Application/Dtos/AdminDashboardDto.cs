namespace Parking.Application.Dtos;

public sealed record AdminDashboardDto
{
    public IReadOnlyCollection<DashboardChartPointDto> WeeklyEntries { get; init; } = Array.Empty<DashboardChartPointDto>();

    public IReadOnlyCollection<DashboardChartPointDto> EntriesByDayOfWeek { get; init; } = Array.Empty<DashboardChartPointDto>();

    public IReadOnlyCollection<DashboardChartPointDto> MonthlyEntries { get; init; } = Array.Empty<DashboardChartPointDto>();

    public IReadOnlyCollection<DashboardChartPointDto> YearlyEntries { get; init; } = Array.Empty<DashboardChartPointDto>();

    public IReadOnlyCollection<DashboardPeriodDto> TrafficByPeriod { get; init; } = Array.Empty<DashboardPeriodDto>();

    public MonthlyTargetDto MonthlyTarget { get; init; } = new();
}

public sealed record DashboardChartPointDto
{
    public string Name { get; init; } = string.Empty;

    public int Value { get; init; }
}

public sealed record DashboardPeriodDto
{
    public string Name { get; init; } = string.Empty;

    public int Entries { get; init; }
}

public sealed record MonthlyTargetDto
{
    public int Year { get; init; }

    public int Month { get; init; }

    public int TargetEntries { get; init; }

    public int ActualEntries { get; init; }

    public int Difference => ActualEntries - TargetEntries;
}

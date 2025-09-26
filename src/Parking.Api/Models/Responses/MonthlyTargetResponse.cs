namespace Parking.Api.Models.Responses;

public sealed record MonthlyTargetResponse
{
    public int Year { get; init; }

    public int Month { get; init; }

    public int TargetEntries { get; init; }

    public int ActualEntries { get; init; }

    public int Difference { get; init; }
}

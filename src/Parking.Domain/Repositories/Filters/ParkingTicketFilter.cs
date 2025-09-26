using System.Collections.Generic;

namespace Parking.Domain.Repositories.Filters;

public sealed class ParkingTicketFilter
{
    public string? PlateEquals { get; init; }

    public string? PlateNotEquals { get; init; }

    public IReadOnlyCollection<string>? PlateIn { get; init; }

    public IReadOnlyCollection<string>? PlateNotIn { get; init; }

    public decimal? TotalAmountEquals { get; init; }

    public decimal? TotalAmountNotEquals { get; init; }

    public decimal? TotalAmountGreaterThan { get; init; }

    public decimal? TotalAmountGreaterThanOrEqual { get; init; }

    public decimal? TotalAmountLessThan { get; init; }

    public decimal? TotalAmountLessThanOrEqual { get; init; }

    public RangeFilter<DateTimeOffset>? EntryAtBetween { get; init; }

    public RangeFilter<DateTimeOffset>? ExitAtNotBetween { get; init; }
}

using System.Collections.Generic;

namespace Parking.Api.Models.Requests;

public sealed record TicketFilterRequest
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

    public DateRangeFilterRequest? EntryAtBetween { get; init; }

    public DateRangeFilterRequest? ExitAtNotBetween { get; init; }
}

public sealed record DateRangeFilterRequest(DateTimeOffset From, DateTimeOffset To);

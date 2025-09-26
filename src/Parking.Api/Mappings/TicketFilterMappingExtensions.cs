using System.Linq;
using Parking.Api.Models.Requests;
using Parking.Domain.Repositories.Filters;

namespace Parking.Api.Mappings;

internal static class TicketFilterMappingExtensions
{
    public static ParkingTicketFilter ToDomainFilter(this TicketFilterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedPlateEquals = NormalizePlate(request.PlateEquals);
        var normalizedPlateNotEquals = NormalizePlate(request.PlateNotEquals);
        var normalizedPlateIn = NormalizePlateCollection(request.PlateIn);
        var normalizedPlateNotIn = NormalizePlateCollection(request.PlateNotIn);

        return new ParkingTicketFilter
        {
            PlateEquals = normalizedPlateEquals,
            PlateNotEquals = normalizedPlateNotEquals,
            PlateIn = normalizedPlateIn,
            PlateNotIn = normalizedPlateNotIn,
            TotalAmountEquals = request.TotalAmountEquals,
            TotalAmountNotEquals = request.TotalAmountNotEquals,
            TotalAmountGreaterThan = request.TotalAmountGreaterThan,
            TotalAmountGreaterThanOrEqual = request.TotalAmountGreaterThanOrEqual,
            TotalAmountLessThan = request.TotalAmountLessThan,
            TotalAmountLessThanOrEqual = request.TotalAmountLessThanOrEqual,
            EntryAtBetween = request.EntryAtBetween is null
                ? null
                : new RangeFilter<DateTimeOffset>(request.EntryAtBetween.From, request.EntryAtBetween.To),
            ExitAtNotBetween = request.ExitAtNotBetween is null
                ? null
                : new RangeFilter<DateTimeOffset>(request.ExitAtNotBetween.From, request.ExitAtNotBetween.To)
        };
    }

    private static string? NormalizePlate(string? plate)
    {
        if (string.IsNullOrWhiteSpace(plate))
        {
            return null;
        }

        return plate.Trim().ToUpperInvariant();
    }

    private static IReadOnlyCollection<string>? NormalizePlateCollection(IReadOnlyCollection<string>? plates)
    {
        if (plates is null)
        {
            return null;
        }

        var normalized = plates
            .Where(plate => !string.IsNullOrWhiteSpace(plate))
            .Select(plate => plate.Trim().ToUpperInvariant())
            .Distinct()
            .ToArray();

        return normalized.Length == 0 ? null : normalized;
    }
}

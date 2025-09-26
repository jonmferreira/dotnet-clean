using Parking.Domain.ValueObjects;

namespace Parking.Domain.Services;

public sealed class ParkingFeeCalculator : IParkingFeeCalculator
{
    private readonly IReadOnlyList<ParkingRate> _rates;

    public static IReadOnlyList<ParkingRate> DefaultRates { get; } = new List<ParkingRate>
    {
        new(TimeSpan.FromMinutes(15), 5.00m),
        new(TimeSpan.FromMinutes(30), 8.00m),
        new(TimeSpan.FromHours(1), 12.00m),
        new(TimeSpan.FromHours(2), 20.00m),
        new(TimeSpan.FromHours(4), 32.00m),
        new(TimeSpan.FromHours(8), 50.00m)
    };

    public ParkingFeeCalculator()
        : this(DefaultRates)
    {
    }

    public ParkingFeeCalculator(IEnumerable<ParkingRate> rates)
    {
        _rates = rates
            .OrderBy(rate => rate.Threshold)
            .ToArray();

        if (_rates.Count == 0)
        {
            throw new ArgumentException("At least one rate must be provided.", nameof(rates));
        }
    }

    public decimal CalculateFee(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            return 0;
        }

        decimal total = 0;

        foreach (var rate in _rates)
        {
            total += rate.Amount;

            if (duration <= rate.Threshold)
            {
                return decimal.Round(total, 2, MidpointRounding.AwayFromZero);
            }
        }

        var lastRate = _rates[^1];
        var extraDuration = duration - lastRate.Threshold;
        if (extraDuration > TimeSpan.Zero)
        {
            var blocks = (int)Math.Ceiling(extraDuration.TotalMinutes / lastRate.Threshold.TotalMinutes);
            total += blocks * lastRate.Amount;
        }

        return decimal.Round(total, 2, MidpointRounding.AwayFromZero);
    }
}

namespace Parking.Domain.ValueObjects;

public sealed record ParkingRate
{
    public ParkingRate(TimeSpan threshold, decimal amount)
    {
        if (threshold <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold must be positive.");
        }

        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative.");
        }

        Threshold = threshold;
        Amount = amount;
    }

    public TimeSpan Threshold { get; }

    public decimal Amount { get; }
}

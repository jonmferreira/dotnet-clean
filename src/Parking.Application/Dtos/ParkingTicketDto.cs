namespace Parking.Application.Dtos;

public sealed record ParkingTicketDto
{
    public Guid Id { get; init; }

    public string Plate { get; init; } = string.Empty;

    public DateTimeOffset EntryAt { get; init; }

    public DateTimeOffset? ExitAt { get; init; }

    public decimal? TotalAmount { get; init; }

    public double? DurationInMinutes { get; init; }
}

namespace Parking.Domain.Entities;

public class ParkingTicket
{
    private ParkingTicket()
    {
        // EF Core constructor
    }

    public ParkingTicket(Guid id, string plate, DateTimeOffset entryAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id must not be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(plate))
        {
            throw new ArgumentException("Plate must not be empty.", nameof(plate));
        }

        Id = id;
        Plate = plate.Trim().ToUpperInvariant();
        EntryAt = entryAt;
    }

    public Guid Id { get; private set; }

    public string Plate { get; private set; } = string.Empty;

    public DateTimeOffset EntryAt { get; private set; }

    public DateTimeOffset? ExitAt { get; private set; }

    public TimeSpan? Duration => ExitAt.HasValue ? ExitAt.Value - EntryAt : null;

    public decimal? TotalAmount { get; private set; }

    public bool IsActive => ExitAt is null;

    public virtual VehicleInspection? Inspection { get; private set; }

    public void Close(DateTimeOffset exitAt, decimal totalAmount)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Ticket is already closed.");
        }

        if (exitAt < EntryAt)
        {
            throw new ArgumentException("Exit date cannot be earlier than entry date.", nameof(exitAt));
        }

        if (totalAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalAmount), "Total amount must be non-negative.");
        }

        ExitAt = exitAt;
        TotalAmount = totalAmount;
    }
}

namespace Parking.Application.Commands;

public sealed record StartParkingCommand(string Plate, DateTimeOffset? EntryAt = null);

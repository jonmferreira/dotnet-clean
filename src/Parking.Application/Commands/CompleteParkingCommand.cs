namespace Parking.Application.Commands;

public sealed record CompleteParkingCommand(Guid TicketId, DateTimeOffset? ExitAt = null);

namespace Parking.Application.Dtos;

public sealed record ParkingTicketDetailsDto
{
    public required ParkingTicketDto Ticket { get; init; }

    public VehicleInspectionDto? Inspection { get; init; }
}

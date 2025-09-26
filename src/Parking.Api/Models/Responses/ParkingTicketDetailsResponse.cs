namespace Parking.Api.Models.Responses;

public sealed record ParkingTicketDetailsResponse
{
    public required ParkingTicketResponse Ticket { get; init; }

    public VehicleInspectionResponse? Inspection { get; init; }

    public required string LoadingStrategy { get; init; }
}

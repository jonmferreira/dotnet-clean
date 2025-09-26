using Parking.Api.Models.Responses;
using Parking.Application.Dtos;

namespace Parking.Api.Mappings;

internal static class TicketDetailsMappingExtensions
{
    public static ParkingTicketDetailsResponse ToResponse(this ParkingTicketDetailsDto dto, string strategy)
    {
        if (dto is null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        return new ParkingTicketDetailsResponse
        {
            Ticket = dto.Ticket.ToResponse(),
            Inspection = dto.Inspection?.ToResponse(),
            LoadingStrategy = strategy
        };
    }
}

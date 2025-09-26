using Parking.Application.Dtos;
using Parking.Domain.Entities;

namespace Parking.Application.Mappings;

internal static class ParkingTicketDetailsMappingExtensions
{
    public static ParkingTicketDetailsDto ToDetailsDto(this ParkingTicket ticket)
    {
        if (ticket is null)
        {
            throw new ArgumentNullException(nameof(ticket));
        }

        return new ParkingTicketDetailsDto
        {
            Ticket = ticket.ToDto(),
            Inspection = ticket.Inspection?.ToDto()
        };
    }
}

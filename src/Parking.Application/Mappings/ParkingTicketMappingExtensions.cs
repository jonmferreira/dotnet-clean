using Parking.Application.Dtos;
using Parking.Domain.Entities;

namespace Parking.Application.Mappings;

internal static class ParkingTicketMappingExtensions
{
    public static ParkingTicketDto ToDto(this ParkingTicket ticket)
    {
        if (ticket is null)
        {
            throw new ArgumentNullException(nameof(ticket));
        }

        return new ParkingTicketDto
        {
            Id = ticket.Id,
            Plate = ticket.Plate,
            EntryAt = ticket.EntryAt,
            ExitAt = ticket.ExitAt,
            TotalAmount = ticket.TotalAmount,
            DurationInMinutes = ticket.Duration?.TotalMinutes
        };
    }
}

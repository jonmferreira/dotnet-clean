using Parking.Api.Models.Responses;
using Parking.Application.Dtos;

namespace Parking.Api.Mappings;

internal static class ParkingTicketMappingExtensions
{
    public static ParkingTicketResponse ToResponse(this ParkingTicketDto dto)
    {
        if (dto is null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        return new ParkingTicketResponse
        {
            Id = dto.Id,
            Plate = dto.Plate,
            EntryAt = dto.EntryAt,
            ExitAt = dto.ExitAt,
            TotalAmount = dto.TotalAmount,
            DurationInMinutes = dto.DurationInMinutes
        };
    }
}

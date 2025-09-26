using Parking.Application.Dtos;

namespace Parking.Api.Services;

public interface ITicketPdfExporter
{
    byte[] Generate(IReadOnlyCollection<ParkingTicketDto> tickets);
}

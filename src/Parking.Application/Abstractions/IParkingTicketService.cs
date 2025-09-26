using Parking.Application.Commands;
using Parking.Application.Dtos;

namespace Parking.Application.Abstractions;

public interface IParkingTicketService
{
    Task<ParkingTicketDto> StartParkingAsync(StartParkingCommand command, CancellationToken cancellationToken = default);

    Task<ParkingTicketDto> CompleteParkingAsync(CompleteParkingCommand command, CancellationToken cancellationToken = default);

    Task<ParkingTicketDto?> GetActiveTicketByPlateAsync(string plate, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ParkingTicketDto>> GetAllTicketsAsync(CancellationToken cancellationToken = default);

    Task<ParkingTicketDto?> GetTicketByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

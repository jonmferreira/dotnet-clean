using Parking.Application.Dtos;

namespace Parking.Application.Abstractions;

public interface ITicketDetailsService
{
    Task<ParkingTicketDetailsDto?> GetWithLazyLoadingAsync(Guid ticketId, CancellationToken cancellationToken = default);

    Task<ParkingTicketDetailsDto?> GetWithEagerLoadingAsync(Guid ticketId, CancellationToken cancellationToken = default);

    Task<ParkingTicketDetailsDto?> GetWithExplicitLoadingAsync(Guid ticketId, CancellationToken cancellationToken = default);
}

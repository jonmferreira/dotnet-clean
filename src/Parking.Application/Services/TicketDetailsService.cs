using Parking.Application.Abstractions;
using Parking.Application.Dtos;
using Parking.Application.Mappings;
using Parking.Domain.Repositories;

namespace Parking.Application.Services;

public sealed class TicketDetailsService : ITicketDetailsService
{
    private readonly IParkingTicketRepository _ticketRepository;

    public TicketDetailsService(IParkingTicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
    }

    public async Task<ParkingTicketDetailsDto?> GetWithLazyLoadingAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        if (ticketId == Guid.Empty)
        {
            return null;
        }

        var ticket = await _ticketRepository.GetByIdWithInspectionLazyAsync(ticketId, cancellationToken);
        return ticket?.ToDetailsDto();
    }

    public async Task<ParkingTicketDetailsDto?> GetWithEagerLoadingAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        if (ticketId == Guid.Empty)
        {
            return null;
        }

        var ticket = await _ticketRepository.GetByIdWithInspectionEagerAsync(ticketId, cancellationToken);
        return ticket?.ToDetailsDto();
    }

    public async Task<ParkingTicketDetailsDto?> GetWithExplicitLoadingAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        if (ticketId == Guid.Empty)
        {
            return null;
        }

        var ticket = await _ticketRepository.GetByIdWithInspectionExplicitAsync(ticketId, cancellationToken);
        return ticket?.ToDetailsDto();
    }
}

using Parking.Domain.Entities;
using Parking.Domain.Repositories.Filters;

namespace Parking.Domain.Repositories;

public interface IParkingTicketRepository
{
    Task<ParkingTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ParkingTicket?> GetByIdWithInspectionLazyAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ParkingTicket?> GetByIdWithInspectionEagerAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ParkingTicket?> GetByIdWithInspectionExplicitAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ParkingTicket?> GetActiveByPlateAsync(string plate, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ParkingTicket>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ParkingTicket>> GetByPeriodAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ParkingTicket>> FilterAsync(ParkingTicketFilter filter, CancellationToken cancellationToken = default);

    Task AddAsync(ParkingTicket ticket, CancellationToken cancellationToken = default);

    Task UpdateAsync(ParkingTicket ticket, CancellationToken cancellationToken = default);
}

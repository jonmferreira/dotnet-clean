using Parking.Domain.Entities;

namespace Parking.Domain.Repositories;

public interface IVehicleInspectionRepository
{
    Task<VehicleInspection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<VehicleInspection?> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);

    Task AddAsync(VehicleInspection inspection, CancellationToken cancellationToken = default);

    Task UpdateAsync(VehicleInspection inspection, CancellationToken cancellationToken = default);
}

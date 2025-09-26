using Microsoft.EntityFrameworkCore;
using Parking.Domain.Entities;
using Parking.Domain.Repositories;
using Parking.Infrastructure.Persistence;

namespace Parking.Infrastructure.Repositories;

public sealed class VehicleInspectionRepository : IVehicleInspectionRepository
{
    private readonly ParkingDbContext _dbContext;

    public VehicleInspectionRepository(ParkingDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(VehicleInspection inspection, CancellationToken cancellationToken = default)
    {
        await _dbContext.VehicleInspections.AddAsync(inspection, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<VehicleInspection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.VehicleInspections
            .FirstOrDefaultAsync(inspection => inspection.Id == id, cancellationToken);
    }

    public async Task<VehicleInspection?> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.VehicleInspections
            .FirstOrDefaultAsync(inspection => inspection.TicketId == ticketId, cancellationToken);
    }

    public async Task UpdateAsync(VehicleInspection inspection, CancellationToken cancellationToken = default)
    {
        _dbContext.VehicleInspections.Update(inspection);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

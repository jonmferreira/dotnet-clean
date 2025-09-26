using Microsoft.EntityFrameworkCore;
using Parking.Domain.Entities;
using Parking.Domain.Repositories;
using Parking.Infrastructure.Persistence;

namespace Parking.Infrastructure.Repositories;

public sealed class MonthlyTargetRepository : IMonthlyTargetRepository
{
    private readonly ParkingDbContext _dbContext;

    public MonthlyTargetRepository(ParkingDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<MonthlyTarget?> GetAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        MonthlyTarget.ValidateYear(year);
        MonthlyTarget.ValidateMonth(month);

        return await _dbContext.MonthlyTargets
            .AsNoTracking()
            .FirstOrDefaultAsync(target => target.Year == year && target.Month == month, cancellationToken);
    }

    public async Task<MonthlyTarget> UpsertAsync(int year, int month, int targetEntries, CancellationToken cancellationToken = default)
    {
        MonthlyTarget.ValidateYear(year);
        MonthlyTarget.ValidateMonth(month);

        if (targetEntries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetEntries), "Target entries must be non-negative.");
        }

        var existing = await _dbContext.MonthlyTargets
            .FirstOrDefaultAsync(target => target.Year == year && target.Month == month, cancellationToken);

        if (existing is null)
        {
            var created = new MonthlyTarget(Guid.NewGuid(), year, month, targetEntries);
            await _dbContext.MonthlyTargets.AddAsync(created, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return created;
        }

        existing.UpdateTarget(targetEntries);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }
}

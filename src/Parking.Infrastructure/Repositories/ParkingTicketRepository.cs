using Microsoft.EntityFrameworkCore;
using Parking.Domain.Entities;
using Parking.Domain.Repositories;
using Parking.Infrastructure.Persistence;

namespace Parking.Infrastructure.Repositories;

public sealed class ParkingTicketRepository : IParkingTicketRepository
{
    private readonly ParkingDbContext _dbContext;

    public ParkingTicketRepository(ParkingDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(ParkingTicket ticket, CancellationToken cancellationToken = default)
    {
        await _dbContext.ParkingTickets.AddAsync(ticket, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ParkingTicket>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ParkingTickets
            .AsNoTracking()
            .OrderByDescending(ticket => ticket.EntryAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ParkingTicket>> GetByPeriodAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default)
    {
        if (to < from)
        {
            throw new ArgumentException("The end date must be greater than or equal to the start date.", nameof(to));
        }

        return await _dbContext.ParkingTickets
            .AsNoTracking()
            .Where(ticket => ticket.EntryAt >= from && ticket.EntryAt < to)
            .ToListAsync(cancellationToken);
    }

    public async Task<ParkingTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ParkingTickets
            .FirstOrDefaultAsync(ticket => ticket.Id == id, cancellationToken);
    }

    public async Task<ParkingTicket?> GetByIdWithInspectionLazyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ParkingTickets
            .FirstOrDefaultAsync(ticket => ticket.Id == id, cancellationToken);
    }

    public async Task<ParkingTicket?> GetByIdWithInspectionEagerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ParkingTickets
            .Include(ticket => ticket.Inspection)
            .FirstOrDefaultAsync(ticket => ticket.Id == id, cancellationToken);
    }

    public async Task<ParkingTicket?> GetByIdWithInspectionExplicitAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ticket = await _dbContext.ParkingTickets
            .FirstOrDefaultAsync(ticket => ticket.Id == id, cancellationToken);

        if (ticket is not null)
        {
            await _dbContext.Entry(ticket)
                .Reference(t => t.Inspection)
                .LoadAsync(cancellationToken);
        }

        return ticket;
    }

    public async Task<ParkingTicket?> GetActiveByPlateAsync(string plate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ParkingTickets
            .FirstOrDefaultAsync(ticket => ticket.Plate == plate && ticket.ExitAt == null, cancellationToken);
    }

    public async Task UpdateAsync(ParkingTicket ticket, CancellationToken cancellationToken = default)
    {
        _dbContext.ParkingTickets.Update(ticket);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

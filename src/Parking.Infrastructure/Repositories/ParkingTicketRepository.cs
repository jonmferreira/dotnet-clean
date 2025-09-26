using System.Linq;
using Microsoft.EntityFrameworkCore;
using Parking.Domain.Entities;
using Parking.Domain.Repositories;
using Parking.Domain.Repositories.Filters;
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

    public async Task<IReadOnlyCollection<ParkingTicket>> FilterAsync(ParkingTicketFilter filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<ParkingTicket> query = _dbContext.ParkingTickets.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.PlateEquals))
        {
            query = query.Where(ticket => ticket.Plate == filter.PlateEquals);
        }

        if (!string.IsNullOrWhiteSpace(filter.PlateNotEquals))
        {
            query = query.Where(ticket => ticket.Plate != filter.PlateNotEquals);
        }

        if (filter.PlateIn is { Count: > 0 })
        {
            query = query.Where(ticket => filter.PlateIn.Contains(ticket.Plate));
        }

        if (filter.PlateNotIn is { Count: > 0 })
        {
            query = query.Where(ticket => !filter.PlateNotIn.Contains(ticket.Plate));
        }

        if (filter.TotalAmountEquals is decimal amountEquals)
        {
            query = query.Where(ticket => ticket.TotalAmount.HasValue && ticket.TotalAmount.Value == amountEquals);
        }

        if (filter.TotalAmountNotEquals is decimal amountNotEquals)
        {
            query = query.Where(ticket => !ticket.TotalAmount.HasValue || ticket.TotalAmount.Value != amountNotEquals);
        }

        if (filter.TotalAmountGreaterThan is decimal amountGreaterThan)
        {
            query = query.Where(ticket => ticket.TotalAmount.HasValue && ticket.TotalAmount.Value > amountGreaterThan);
        }

        if (filter.TotalAmountGreaterThanOrEqual is decimal amountGreaterThanOrEqual)
        {
            query = query.Where(ticket => ticket.TotalAmount.HasValue && ticket.TotalAmount.Value >= amountGreaterThanOrEqual);
        }

        if (filter.TotalAmountLessThan is decimal amountLessThan)
        {
            query = query.Where(ticket => ticket.TotalAmount.HasValue && ticket.TotalAmount.Value < amountLessThan);
        }

        if (filter.TotalAmountLessThanOrEqual is decimal amountLessThanOrEqual)
        {
            query = query.Where(ticket => ticket.TotalAmount.HasValue && ticket.TotalAmount.Value <= amountLessThanOrEqual);
        }

        if (filter.EntryAtBetween is not null)
        {
            query = query.Where(ticket => ticket.EntryAt >= filter.EntryAtBetween.From && ticket.EntryAt <= filter.EntryAtBetween.To);
        }

        if (filter.ExitAtNotBetween is not null)
        {
            query = query.Where(ticket => !ticket.ExitAt.HasValue || ticket.ExitAt.Value < filter.ExitAtNotBetween.From || ticket.ExitAt.Value > filter.ExitAtNotBetween.To);
        }

        return await query
            .OrderByDescending(ticket => ticket.EntryAt)
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

using Parking.Application.Abstractions;
using Parking.Application.Commands;
using Parking.Application.Dtos;
using Parking.Application.Mappings;
using Parking.Domain.Entities;
using Parking.Domain.Repositories;
using Parking.Domain.Repositories.Filters;
using Parking.Domain.Services;

namespace Parking.Application.Services;

public sealed class ParkingTicketService : IParkingTicketService
{
    private readonly IParkingTicketRepository _repository;
    private readonly IParkingFeeCalculator _feeCalculator;

    public ParkingTicketService(IParkingTicketRepository repository, IParkingFeeCalculator feeCalculator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _feeCalculator = feeCalculator ?? throw new ArgumentNullException(nameof(feeCalculator));
    }

    public async Task<ParkingTicketDto> StartParkingAsync(StartParkingCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Plate))
        {
            throw new ArgumentException("The vehicle plate must be informed.", nameof(command));
        }

        var normalizedPlate = command.Plate.Trim().ToUpperInvariant();
        var existing = await _repository.GetActiveByPlateAsync(normalizedPlate, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException($"There is already an active ticket for plate {normalizedPlate}.");
        }

        var entryAt = command.EntryAt ?? DateTimeOffset.UtcNow;
        var ticket = new ParkingTicket(Guid.NewGuid(), normalizedPlate, entryAt);

        await _repository.AddAsync(ticket, cancellationToken);

        return ticket.ToDto();
    }

    public async Task<ParkingTicketDto> CompleteParkingAsync(CompleteParkingCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ticket = await _repository.GetByIdAsync(command.TicketId, cancellationToken)
            ?? throw new KeyNotFoundException($"Ticket {command.TicketId} was not found.");

        if (!ticket.IsActive)
        {
            return ticket.ToDto();
        }

        var exitAt = command.ExitAt ?? DateTimeOffset.UtcNow;
        var duration = exitAt - ticket.EntryAt;
        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentException("The exit date cannot be before the entry date.", nameof(command));
        }

        var total = _feeCalculator.CalculateFee(duration);
        ticket.Close(exitAt, total);
        await _repository.UpdateAsync(ticket, cancellationToken);

        return ticket.ToDto();
    }

    public async Task<ParkingTicketDto?> GetActiveTicketByPlateAsync(string plate, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(plate))
        {
            return null;
        }

        var normalizedPlate = plate.Trim().ToUpperInvariant();
        var ticket = await _repository.GetActiveByPlateAsync(normalizedPlate, cancellationToken);
        return ticket?.ToDto();
    }

    public async Task<IReadOnlyCollection<ParkingTicketDto>> GetAllTicketsAsync(CancellationToken cancellationToken = default)
    {
        var tickets = await _repository.GetAllAsync(cancellationToken);
        return tickets.Select(ticket => ticket.ToDto()).ToArray();
    }

    public async Task<ParkingTicketDto?> GetTicketByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ticket = await _repository.GetByIdAsync(id, cancellationToken);
        return ticket?.ToDto();
    }

    public async Task<IReadOnlyCollection<ParkingTicketDto>> FilterTicketsAsync(ParkingTicketFilter filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var tickets = await _repository.FilterAsync(filter, cancellationToken);
        return tickets.Select(ticket => ticket.ToDto()).ToArray();
    }
}

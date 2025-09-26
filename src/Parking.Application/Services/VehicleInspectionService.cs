using Parking.Application.Abstractions;
using Parking.Application.Commands;
using Parking.Application.Dtos;
using Parking.Application.Mappings;
using Parking.Domain.Entities;
using Parking.Domain.Repositories;

namespace Parking.Application.Services;

public sealed class VehicleInspectionService : IVehicleInspectionService
{
    private readonly IVehicleInspectionRepository _inspectionRepository;
    private readonly IParkingTicketRepository _ticketRepository;

    public VehicleInspectionService(
        IVehicleInspectionRepository inspectionRepository,
        IParkingTicketRepository ticketRepository)
    {
        _inspectionRepository = inspectionRepository ?? throw new ArgumentNullException(nameof(inspectionRepository));
        _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
    }

    public async Task<VehicleInspectionDto> CreateInspectionAsync(CreateVehicleInspectionCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.TicketId == Guid.Empty)
        {
            throw new ArgumentException("The ticket id must be informed.", nameof(command));
        }

        var ticket = await _ticketRepository.GetByIdAsync(command.TicketId, cancellationToken)
            ?? throw new KeyNotFoundException($"Ticket {command.TicketId} was not found.");

        var existingInspection = await _inspectionRepository.GetByTicketIdAsync(ticket.Id, cancellationToken);
        if (existingInspection is not null)
        {
            throw new InvalidOperationException($"An inspection has already been registered for ticket {ticket.Id}.");
        }

        var inspection = new VehicleInspection(
            Guid.NewGuid(),
            ticket.Id,
            command.NoScratches,
            command.ScratchesPhotoUrl,
            command.NoMissingItems,
            command.MissingItemsPhotoUrl,
            command.NoLostKeys,
            command.LostKeysPhotoUrl,
            command.NoHarshImpacts,
            command.HarshImpactsPhotoUrl,
            command.InspectedAt);

        await _inspectionRepository.AddAsync(inspection, cancellationToken);

        return inspection.ToDto();
    }

    public async Task<VehicleInspectionDto?> GetInspectionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var inspection = await _inspectionRepository.GetByIdAsync(id, cancellationToken);
        return inspection?.ToDto();
    }

    public async Task<VehicleInspectionDto?> GetInspectionByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        if (ticketId == Guid.Empty)
        {
            return null;
        }

        var inspection = await _inspectionRepository.GetByTicketIdAsync(ticketId, cancellationToken);
        return inspection?.ToDto();
    }

    public async Task<VehicleInspectionDto> UpdateInspectionAsync(UpdateVehicleInspectionCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.InspectionId == Guid.Empty)
        {
            throw new ArgumentException("The inspection id must be informed.", nameof(command));
        }

        var inspection = await _inspectionRepository.GetByIdAsync(command.InspectionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Inspection {command.InspectionId} was not found.");

        inspection.UpdateChecklist(
            command.NoScratches,
            command.ScratchesPhotoUrl,
            command.NoMissingItems,
            command.MissingItemsPhotoUrl,
            command.NoLostKeys,
            command.LostKeysPhotoUrl,
            command.NoHarshImpacts,
            command.HarshImpactsPhotoUrl,
            command.InspectedAt);

        await _inspectionRepository.UpdateAsync(inspection, cancellationToken);

        return inspection.ToDto();
    }
}

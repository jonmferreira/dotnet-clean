using Parking.Application.Commands;
using Parking.Application.Dtos;

namespace Parking.Application.Abstractions;

public interface IVehicleInspectionService
{
    Task<VehicleInspectionDto> CreateInspectionAsync(CreateVehicleInspectionCommand command, CancellationToken cancellationToken = default);

    Task<VehicleInspectionDto> UpdateInspectionAsync(UpdateVehicleInspectionCommand command, CancellationToken cancellationToken = default);

    Task<VehicleInspectionDto?> GetInspectionByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<VehicleInspectionDto?> GetInspectionByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
}

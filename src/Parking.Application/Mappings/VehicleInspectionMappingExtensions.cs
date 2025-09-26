using Parking.Application.Dtos;
using Parking.Domain.Entities;

namespace Parking.Application.Mappings;

internal static class VehicleInspectionMappingExtensions
{
    public static VehicleInspectionDto ToDto(this VehicleInspection inspection)
    {
        if (inspection is null)
        {
            throw new ArgumentNullException(nameof(inspection));
        }

        return new VehicleInspectionDto
        {
            Id = inspection.Id,
            TicketId = inspection.TicketId,
            InspectedAt = inspection.InspectedAt,
            NoScratches = inspection.NoScratches,
            ScratchesPhotoUrl = inspection.ScratchesPhotoUrl,
            NoMissingItems = inspection.NoMissingItems,
            MissingItemsPhotoUrl = inspection.MissingItemsPhotoUrl,
            NoLostKeys = inspection.NoLostKeys,
            LostKeysPhotoUrl = inspection.LostKeysPhotoUrl,
            NoHarshImpacts = inspection.NoHarshImpacts,
            HarshImpactsPhotoUrl = inspection.HarshImpactsPhotoUrl
        };
    }
}

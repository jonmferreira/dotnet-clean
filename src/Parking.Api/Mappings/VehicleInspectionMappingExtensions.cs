using Parking.Api.Models.Responses;
using Parking.Application.Dtos;

namespace Parking.Api.Mappings;

internal static class VehicleInspectionMappingExtensions
{
    public static VehicleInspectionResponse ToResponse(this VehicleInspectionDto dto)
    {
        if (dto is null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        return new VehicleInspectionResponse
        {
            Id = dto.Id,
            TicketId = dto.TicketId,
            InspectedAt = dto.InspectedAt,
            NoScratches = dto.NoScratches,
            ScratchesPhotoUrl = dto.ScratchesPhotoUrl,
            NoMissingItems = dto.NoMissingItems,
            MissingItemsPhotoUrl = dto.MissingItemsPhotoUrl,
            NoLostKeys = dto.NoLostKeys,
            LostKeysPhotoUrl = dto.LostKeysPhotoUrl,
            NoHarshImpacts = dto.NoHarshImpacts,
            HarshImpactsPhotoUrl = dto.HarshImpactsPhotoUrl
        };
    }
}

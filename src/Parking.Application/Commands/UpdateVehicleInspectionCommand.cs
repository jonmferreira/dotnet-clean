namespace Parking.Application.Commands;

public sealed record UpdateVehicleInspectionCommand(
    Guid InspectionId,
    bool NoScratches,
    string? ScratchesPhotoUrl,
    bool NoMissingItems,
    string? MissingItemsPhotoUrl,
    bool NoLostKeys,
    string? LostKeysPhotoUrl,
    bool NoHarshImpacts,
    string? HarshImpactsPhotoUrl,
    DateTimeOffset? InspectedAt = null);

namespace Parking.Api.Models.Requests;

public abstract class VehicleInspectionChecklistRequestBase
{
    public bool NoScratches { get; set; } = true;

    public string? ScratchesPhotoUrl { get; set; }

    public bool NoMissingItems { get; set; } = true;

    public string? MissingItemsPhotoUrl { get; set; }

    public bool NoLostKeys { get; set; } = true;

    public string? LostKeysPhotoUrl { get; set; }

    public bool NoHarshImpacts { get; set; } = true;

    public string? HarshImpactsPhotoUrl { get; set; }

    public DateTimeOffset? InspectedAt { get; set; }
}

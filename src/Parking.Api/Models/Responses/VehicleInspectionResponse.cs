namespace Parking.Api.Models.Responses;

public sealed class VehicleInspectionResponse
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public DateTimeOffset InspectedAt { get; set; }

    public bool NoScratches { get; set; }

    public string? ScratchesPhotoUrl { get; set; }

    public bool NoMissingItems { get; set; }

    public string? MissingItemsPhotoUrl { get; set; }

    public bool NoLostKeys { get; set; }

    public string? LostKeysPhotoUrl { get; set; }

    public bool NoHarshImpacts { get; set; }

    public string? HarshImpactsPhotoUrl { get; set; }
}

namespace Parking.Application.Dtos;

public sealed record VehicleInspectionDto
{
    public Guid Id { get; init; }

    public Guid TicketId { get; init; }

    public DateTimeOffset InspectedAt { get; init; }

    public bool NoScratches { get; init; }

    public string? ScratchesPhotoUrl { get; init; }

    public bool NoMissingItems { get; init; }

    public string? MissingItemsPhotoUrl { get; init; }

    public bool NoLostKeys { get; init; }

    public string? LostKeysPhotoUrl { get; init; }

    public bool NoHarshImpacts { get; init; }

    public string? HarshImpactsPhotoUrl { get; init; }
}

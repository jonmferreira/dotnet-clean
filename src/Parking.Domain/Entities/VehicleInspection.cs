namespace Parking.Domain.Entities;

public class VehicleInspection
{
    private VehicleInspection()
    {
        // EF Core constructor
    }

    public VehicleInspection(
        Guid id,
        Guid ticketId,
        bool noScratches,
        string? scratchesPhotoUrl,
        bool noMissingItems,
        string? missingItemsPhotoUrl,
        bool noLostKeys,
        string? lostKeysPhotoUrl,
        bool noHarshImpacts,
        string? harshImpactsPhotoUrl,
        DateTimeOffset? inspectedAt = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id must not be empty.", nameof(id));
        }

        if (ticketId == Guid.Empty)
        {
            throw new ArgumentException("TicketId must not be empty.", nameof(ticketId));
        }

        Id = id;
        TicketId = ticketId;
        UpdateChecklist(
            noScratches,
            scratchesPhotoUrl,
            noMissingItems,
            missingItemsPhotoUrl,
            noLostKeys,
            lostKeysPhotoUrl,
            noHarshImpacts,
            harshImpactsPhotoUrl,
            inspectedAt ?? DateTimeOffset.UtcNow);
    }

    public Guid Id { get; private set; }

    public Guid TicketId { get; private set; }

    public DateTimeOffset InspectedAt { get; private set; }

    public bool NoScratches { get; private set; }

    public string? ScratchesPhotoUrl { get; private set; }

    public bool NoMissingItems { get; private set; }

    public string? MissingItemsPhotoUrl { get; private set; }

    public bool NoLostKeys { get; private set; }

    public string? LostKeysPhotoUrl { get; private set; }

    public bool NoHarshImpacts { get; private set; }

    public string? HarshImpactsPhotoUrl { get; private set; }

    public virtual ParkingTicket? Ticket { get; private set; }

    public void UpdateChecklist(
        bool noScratches,
        string? scratchesPhotoUrl,
        bool noMissingItems,
        string? missingItemsPhotoUrl,
        bool noLostKeys,
        string? lostKeysPhotoUrl,
        bool noHarshImpacts,
        string? harshImpactsPhotoUrl,
        DateTimeOffset? inspectedAt = null)
    {
        NoScratches = noScratches;
        ScratchesPhotoUrl = NormalizeEvidence(noScratches, scratchesPhotoUrl, nameof(scratchesPhotoUrl));

        NoMissingItems = noMissingItems;
        MissingItemsPhotoUrl = NormalizeEvidence(noMissingItems, missingItemsPhotoUrl, nameof(missingItemsPhotoUrl));

        NoLostKeys = noLostKeys;
        LostKeysPhotoUrl = NormalizeEvidence(noLostKeys, lostKeysPhotoUrl, nameof(lostKeysPhotoUrl));

        NoHarshImpacts = noHarshImpacts;
        HarshImpactsPhotoUrl = NormalizeEvidence(noHarshImpacts, harshImpactsPhotoUrl, nameof(harshImpactsPhotoUrl));

        InspectedAt = inspectedAt ?? DateTimeOffset.UtcNow;
    }

    private static string? NormalizeEvidence(bool isApproved, string? photoUrl, string parameterName)
    {
        if (isApproved)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(photoUrl))
        {
            throw new ArgumentException("A photo must be provided when the checklist item is not approved.", parameterName);
        }

        return photoUrl.Trim();
    }
}

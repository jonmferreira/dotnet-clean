using Parking.Domain.Entities;

namespace Parking.UnitTests.Entities;

public class VehicleInspectionTests
{
    [Fact]
    public void Constructor_WhenItemNotApprovedRequiresPhoto()
    {
        var id = Guid.NewGuid();
        var ticketId = Guid.NewGuid();

        var exception = Assert.Throws<ArgumentException>(() => new VehicleInspection(
            id,
            ticketId,
            noScratches: false,
            scratchesPhotoUrl: null,
            noMissingItems: true,
            missingItemsPhotoUrl: null,
            noLostKeys: true,
            lostKeysPhotoUrl: null,
            noHarshImpacts: true,
            harshImpactsPhotoUrl: null));

        Assert.Equal("scratchesPhotoUrl", exception.ParamName);
    }

    [Fact]
    public void UpdateChecklist_WhenMarkingItemAsApproved_RemovesPhoto()
    {
        var id = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var inspection = new VehicleInspection(
            id,
            ticketId,
            noScratches: false,
            scratchesPhotoUrl: "https://example.com/scratch.jpg",
            noMissingItems: true,
            missingItemsPhotoUrl: null,
            noLostKeys: true,
            lostKeysPhotoUrl: null,
            noHarshImpacts: true,
            harshImpactsPhotoUrl: null,
            inspectedAt: DateTimeOffset.UtcNow.AddMinutes(-5));

        inspection.UpdateChecklist(
            noScratches: true,
            scratchesPhotoUrl: "https://example.com/should-be-cleared.jpg",
            noMissingItems: true,
            missingItemsPhotoUrl: null,
            noLostKeys: true,
            lostKeysPhotoUrl: null,
            noHarshImpacts: true,
            harshImpactsPhotoUrl: null,
            inspectedAt: DateTimeOffset.UtcNow);

        Assert.True(inspection.NoScratches);
        Assert.Null(inspection.ScratchesPhotoUrl);
    }
}

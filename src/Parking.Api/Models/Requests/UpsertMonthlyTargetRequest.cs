using System.ComponentModel.DataAnnotations;

namespace Parking.Api.Models.Requests;

public sealed class UpsertMonthlyTargetRequest
{
    [Range(1, int.MaxValue)]
    public int Year { get; set; }

    [Range(1, 12)]
    public int Month { get; set; }

    [Range(0, int.MaxValue)]
    public int TargetEntries { get; set; }
}

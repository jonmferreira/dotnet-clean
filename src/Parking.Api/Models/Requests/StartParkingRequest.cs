using System.ComponentModel.DataAnnotations;

namespace Parking.Api.Models.Requests;

public sealed class StartParkingRequest
{
    [Required]
    [MaxLength(16)]
    public string Plate { get; set; } = string.Empty;

    public DateTimeOffset? EntryAt { get; set; }
}

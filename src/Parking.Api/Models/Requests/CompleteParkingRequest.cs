namespace Parking.Api.Models.Requests;

public sealed class CompleteParkingRequest
{
    public DateTimeOffset? ExitAt { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace Parking.Api.Models.Requests;

public sealed class CreateVehicleInspectionRequest : VehicleInspectionChecklistRequestBase
{
    [Required]
    public Guid TicketId { get; set; }
}

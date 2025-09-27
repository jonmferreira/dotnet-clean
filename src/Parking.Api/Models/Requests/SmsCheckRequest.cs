using System.ComponentModel.DataAnnotations;

namespace Parking.Api.Models.Requests;

public sealed class SmsCheckRequest
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(160)]
    public string Message { get; set; } = "Mensagem de teste do estacionamento.";
}

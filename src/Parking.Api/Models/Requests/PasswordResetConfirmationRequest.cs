using System.ComponentModel.DataAnnotations;

namespace Parking.Api.Models.Requests;

public sealed class PasswordResetConfirmationRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}

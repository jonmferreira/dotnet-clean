using System.ComponentModel.DataAnnotations;

namespace Parking.Api.Models.Requests;

public sealed class PasswordResetRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

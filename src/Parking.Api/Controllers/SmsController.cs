using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parking.Api.Models.Requests;
using Parking.Application.Abstractions;

namespace Parking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class SmsController : ControllerBase
{
    private readonly ISmsSender _smsSender;
    private readonly ILogger<SmsController> _logger;

    public SmsController(ISmsSender smsSender, ILogger<SmsController> logger)
    {
        _smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("check")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> SendCheckAsync([FromBody] SmsCheckRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            await _smsSender.SendAsync(request.PhoneNumber, request.Message, cancellationToken);
            return Accepted(new { request.PhoneNumber, request.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", request.PhoneNumber);
            return Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "Failed to send SMS.",
                detail: "Não foi possível enviar a mensagem via SNS. Verifique as configurações e os logs para mais detalhes.");
        }
    }
}

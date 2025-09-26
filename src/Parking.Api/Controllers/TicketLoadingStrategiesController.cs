using Microsoft.AspNetCore.Mvc;
using Parking.Api.Mappings;
using Parking.Api.Models.Responses;
using Parking.Application.Abstractions;

namespace Parking.Api.Controllers;

[ApiController]
[Route("api/tickets/{ticketId:guid}/inspection-strategies")]
public sealed class TicketLoadingStrategiesController : ControllerBase
{
    private readonly ITicketDetailsService _ticketDetailsService;

    public TicketLoadingStrategiesController(ITicketDetailsService ticketDetailsService)
    {
        _ticketDetailsService = ticketDetailsService ?? throw new ArgumentNullException(nameof(ticketDetailsService));
    }

    [HttpGet("lazy")]
    [ProducesResponseType(typeof(ParkingTicketDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParkingTicketDetailsResponse>> GetWithLazyLoadingAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        var details = await _ticketDetailsService.GetWithLazyLoadingAsync(ticketId, cancellationToken);
        return details is null
            ? NotFound()
            : Ok(details.ToResponse("Lazy Loading"));
    }

    [HttpGet("eager")]
    [ProducesResponseType(typeof(ParkingTicketDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParkingTicketDetailsResponse>> GetWithEagerLoadingAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        var details = await _ticketDetailsService.GetWithEagerLoadingAsync(ticketId, cancellationToken);
        return details is null
            ? NotFound()
            : Ok(details.ToResponse("Eager Loading"));
    }

    [HttpGet("explicit")]
    [ProducesResponseType(typeof(ParkingTicketDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParkingTicketDetailsResponse>> GetWithExplicitLoadingAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        var details = await _ticketDetailsService.GetWithExplicitLoadingAsync(ticketId, cancellationToken);
        return details is null
            ? NotFound()
            : Ok(details.ToResponse("Explicit Loading"));
    }
}

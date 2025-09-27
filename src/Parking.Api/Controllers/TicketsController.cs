using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parking.Api.Mappings;
using Parking.Api.Models.Requests;
using Parking.Api.Models.Responses;
using Parking.Api.Services;
using Parking.Application.Abstractions;
using Parking.Application.Commands;

namespace Parking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TicketsController : ControllerBase
{
    private readonly IParkingTicketService _parkingTicketService;
    private readonly ITicketPdfExporter _ticketPdfExporter;

    public TicketsController(IParkingTicketService parkingTicketService, ITicketPdfExporter ticketPdfExporter)
    {
        _parkingTicketService = parkingTicketService ?? throw new ArgumentNullException(nameof(parkingTicketService));
        _ticketPdfExporter = ticketPdfExporter ?? throw new ArgumentNullException(nameof(ticketPdfExporter));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ParkingTicketResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ParkingTicketResponse>> CreateAsync([FromBody] StartParkingRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var command = new StartParkingCommand(request.Plate, request.EntryAt);
            var ticket = await _parkingTicketService.StartParkingAsync(command, cancellationToken);
            var response = ticket.ToResponse();
            return CreatedAtAction(nameof(GetByIdAsync), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(ParkingTicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParkingTicketResponse>> CompleteAsync(Guid id, [FromBody] CompleteParkingRequest? request, CancellationToken cancellationToken)
    {
        try
        {
            request ??= new CompleteParkingRequest();
            var command = new CompleteParkingCommand(id, request.ExitAt);
            var ticket = await _parkingTicketService.CompleteParkingAsync(command, cancellationToken);
            return Ok(ticket.ToResponse());
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ParkingTicketResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ParkingTicketResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var tickets = await _parkingTicketService.GetAllTicketsAsync(cancellationToken);
        return Ok(tickets.Select(ticket => ticket.ToResponse()));
    }


    [HttpGet("export/pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportAsPdfAsync(CancellationToken cancellationToken)
    {
        var tickets = await _parkingTicketService.GetAllTicketsAsync(cancellationToken);
        var pdfBytes = _ticketPdfExporter.Generate(tickets);
        var fileName = $"tickets-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
    
    [HttpPost("filter")]
    [ProducesResponseType(typeof(IEnumerable<ParkingTicketResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ParkingTicketResponse>>> FilterAsync([FromBody] TicketFilterRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("A filter request body is required.");
        }

        try
        {
            var filter = request.ToDomainFilter();
            var tickets = await _parkingTicketService.FilterTicketsAsync(filter, cancellationToken);
            return Ok(tickets.Select(ticket => ticket.ToResponse()));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ParkingTicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParkingTicketResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await _parkingTicketService.GetTicketByIdAsync(id, cancellationToken);
        return ticket is null ? NotFound() : Ok(ticket.ToResponse());
    }

    [HttpGet("active/{plate}")]
    [ProducesResponseType(typeof(ParkingTicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParkingTicketResponse>> GetActiveByPlateAsync(string plate, CancellationToken cancellationToken)
    {
        var ticket = await _parkingTicketService.GetActiveTicketByPlateAsync(plate, cancellationToken);
        return ticket is null ? NotFound() : Ok(ticket.ToResponse());
    }
}

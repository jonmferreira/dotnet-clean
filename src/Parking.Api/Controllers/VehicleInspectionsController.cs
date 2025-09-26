using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Parking.Api.Mappings;
using Parking.Api.Models.Requests;
using Parking.Api.Models.Responses;
using Parking.Application.Abstractions;
using Parking.Application.Commands;

namespace Parking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class VehicleInspectionsController : ControllerBase
{
    private readonly IVehicleInspectionService _vehicleInspectionService;

    public VehicleInspectionsController(IVehicleInspectionService vehicleInspectionService)
    {
        _vehicleInspectionService = vehicleInspectionService ?? throw new ArgumentNullException(nameof(vehicleInspectionService));
    }

    [HttpPost]
    [ProducesResponseType(typeof(VehicleInspectionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VehicleInspectionResponse>> CreateAsync([FromBody] CreateVehicleInspectionRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        ValidateEvidence(request, ModelState);
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var command = new CreateVehicleInspectionCommand(
                request.TicketId,
                request.NoScratches,
                request.ScratchesPhotoUrl,
                request.NoMissingItems,
                request.MissingItemsPhotoUrl,
                request.NoLostKeys,
                request.LostKeysPhotoUrl,
                request.NoHarshImpacts,
                request.HarshImpactsPhotoUrl,
                request.InspectedAt);

            var inspection = await _vehicleInspectionService.CreateInspectionAsync(command, cancellationToken);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = inspection.Id }, inspection.ToResponse());
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(VehicleInspectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleInspectionResponse>> UpdateAsync(Guid id, [FromBody] UpdateVehicleInspectionRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        ValidateEvidence(request, ModelState);
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var command = new UpdateVehicleInspectionCommand(
                id,
                request.NoScratches,
                request.ScratchesPhotoUrl,
                request.NoMissingItems,
                request.MissingItemsPhotoUrl,
                request.NoLostKeys,
                request.LostKeysPhotoUrl,
                request.NoHarshImpacts,
                request.HarshImpactsPhotoUrl,
                request.InspectedAt);

            var inspection = await _vehicleInspectionService.UpdateInspectionAsync(command, cancellationToken);
            return Ok(inspection.ToResponse());
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

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VehicleInspectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleInspectionResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var inspection = await _vehicleInspectionService.GetInspectionByIdAsync(id, cancellationToken);
        return inspection is null ? NotFound() : Ok(inspection.ToResponse());
    }

    [HttpGet("ticket/{ticketId:guid}")]
    [ProducesResponseType(typeof(VehicleInspectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleInspectionResponse>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        var inspection = await _vehicleInspectionService.GetInspectionByTicketIdAsync(ticketId, cancellationToken);
        return inspection is null ? NotFound() : Ok(inspection.ToResponse());
    }

    private static void ValidateEvidence(VehicleInspectionChecklistRequestBase request, ModelStateDictionary modelState)
    {
        ValidateItem(modelState, nameof(request.ScratchesPhotoUrl), request.NoScratches, request.ScratchesPhotoUrl);
        ValidateItem(modelState, nameof(request.MissingItemsPhotoUrl), request.NoMissingItems, request.MissingItemsPhotoUrl);
        ValidateItem(modelState, nameof(request.LostKeysPhotoUrl), request.NoLostKeys, request.LostKeysPhotoUrl);
        ValidateItem(modelState, nameof(request.HarshImpactsPhotoUrl), request.NoHarshImpacts, request.HarshImpactsPhotoUrl);
    }

    private static void ValidateItem(ModelStateDictionary modelState, string propertyName, bool isApproved, string? photoUrl)
    {
        if (!isApproved && string.IsNullOrWhiteSpace(photoUrl))
        {
            modelState.AddModelError(propertyName, "A photo URL must be provided when the checklist item is not approved.");
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parking.Api.Mappings;
using Parking.Api.Models.Requests;
using Parking.Api.Models.Responses;
using Parking.Application.Abstractions;

namespace Parking.Api.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize]
public sealed class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _adminDashboardService;

    public AdminDashboardController(IAdminDashboardService adminDashboardService)
    {
        _adminDashboardService = adminDashboardService ?? throw new ArgumentNullException(nameof(adminDashboardService));
    }

    [HttpGet("metrics")]
    [ProducesResponseType(typeof(AdminDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminDashboardResponse>> GetMetricsAsync([FromQuery] int? year, [FromQuery] int? month, CancellationToken cancellationToken)
    {
        var current = DateTimeOffset.UtcNow;
        var targetYear = year ?? current.Year;
        var targetMonth = month ?? current.Month;

        try
        {
            var dashboard = await _adminDashboardService.GetMetricsAsync(targetYear, targetMonth, cancellationToken);
            return Ok(dashboard.ToResponse());
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("targets")]
    [ProducesResponseType(typeof(MonthlyTargetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MonthlyTargetResponse>> UpsertMonthlyTargetAsync([FromBody] UpsertMonthlyTargetRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var target = await _adminDashboardService.SetMonthlyTargetAsync(request.Year, request.Month, request.TargetEntries, cancellationToken);
            return Ok(target.ToResponse());
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

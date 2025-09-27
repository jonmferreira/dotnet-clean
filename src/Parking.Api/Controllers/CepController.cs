using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parking.Application.Abstractions;
using Parking.Application.Dtos;

namespace Parking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public sealed class CepController : ControllerBase
{
    private readonly ICepLookupService _cepLookupService;

    public CepController(ICepLookupService cepLookupService)
    {
        _cepLookupService = cepLookupService ?? throw new ArgumentNullException(nameof(cepLookupService));
    }

    [HttpGet("{cep}")]
    [ProducesResponseType(typeof(CepAddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetAsync(string cep, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cep))
        {
            return BadRequest("CEP must be provided.");
        }

        try
        {
            var address = await _cepLookupService.GetAddressByCepAsync(cep, cancellationToken);
            return address is null ? NotFound() : Ok(address);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Unable to reach CEP service at the moment.");
        }
    }
}

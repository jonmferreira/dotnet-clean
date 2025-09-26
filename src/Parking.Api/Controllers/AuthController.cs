using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parking.Api.Models.Requests;
using Parking.Api.Models.Responses;
using Parking.Application.Abstractions;
using Parking.Application.Dtos;

namespace Parking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var loginDto = new LoginRequestDto(request.Email, request.Password);
            var result = await _authService.LoginAsync(loginDto, cancellationToken);

            var response = new LoginResponse
            {
                AccessToken = result.AccessToken,
                ExpiresAt = result.ExpiresAt,
                User = new AuthenticatedUserResponse
                {
                    Id = result.UserId,
                    Name = result.Name,
                    Email = result.Email,
                    Role = result.Role
                }
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}

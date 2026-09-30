using Gclopes.FinVault.Identity.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Gclopes.FinVault.Identity.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Auth Service is operational.");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var succeeded = await mediator.Send(command);

        if (!succeeded)
        {
            return BadRequest("Registration failed.");
        }

        return Ok(new { Message = "User registered successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var result = await mediator.Send(command);
        if (result == null)
        {
            return Problem(
                detail: "Invalid username or password",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized"
            );
        }

        return Ok(result); // Devolve o objeto plano diretamente
    }
}
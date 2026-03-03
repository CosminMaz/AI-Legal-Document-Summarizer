using Cosmin.Application.Users.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Cosmin.API.Controllers;

[ApiController]
[Route("/api/v1/users/")]
public class UsersController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    
    [HttpPost("register")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var userId = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(RegisterUser), new { id = userId }, userId);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            var message = pg.ConstraintName switch
            {
                "users_username_key" => "Username already exists.",
                "users_email_key" => "Email already exists.",
                _ => "A user with the same username or email already exists."
            };

            return BadRequest(new { message });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginUser([FromBody] LoginUserCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);

        if (response is null)
        {
            return Unauthorized(new { message = "Invalid username/email or password." });
        }

        return Ok(response);
    }
}
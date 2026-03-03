using MediatR;

namespace Cosmin.Application.Users.Commands;

public sealed record LoginUserCommand(string UsernameOrEmail, string Password) : IRequest<LoginUserResponse?>;

public sealed record LoginUserResponse(Guid Id, string Username, string Email, string Role);


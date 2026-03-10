using Cosmin.Domain.Enums;
using MediatR;

namespace Cosmin.Application.Users.Commands;

public record RegisterUserCommand(string Username, string Email, string Password, UserRole Role) : IRequest<Guid>
{
    
}

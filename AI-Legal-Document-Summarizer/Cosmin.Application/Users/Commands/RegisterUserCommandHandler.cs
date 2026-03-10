using Cosmin.Application.Abstractions;
using Cosmin.Domain.Entities;
using MediatR;

namespace Cosmin.Application.Users.Commands;

public sealed class RegisterUserCommandHandler: IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _repository;
    public RegisterUserCommandHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Hash the password using BCrypt
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
        
        var user = User.Create(request.Username, request.Email, hashedPassword, request.Role);
        await _repository.AddAsync(user, cancellationToken);
        return user.Id;
    }
}
using Cosmin.Application.Abstractions;
using MediatR;

namespace Cosmin.Application.Users.Commands;

public sealed class LoginUserCommandHandler(IUserRepository repository) : IRequestHandler<LoginUserCommand, LoginUserResponse?>
{
    private readonly IUserRepository _repository = repository;

    public async Task<LoginUserResponse?> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByUsernameOrEmailAsync(request.UsernameOrEmail, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var validPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
        if (!validPassword)
        {
            return null;
        }

        return new LoginUserResponse(user.Id, user.Username, user.Email, user.Role.ToString());
    }
}


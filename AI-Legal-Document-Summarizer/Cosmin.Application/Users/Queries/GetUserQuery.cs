    using Cosmin.Application.Abstractions;
using MediatR;

namespace Cosmin.Application.Users.Queries;

public record GetUserQuery(Guid Id) : IRequest<UserDto?>;

public sealed class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto?>
{
    private readonly IUserRepository _repository;

    public GetUserQueryHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserDto?> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (user is null)
        {
            return null;
        }

        return new UserDto(user.Id, user.Username, user.Email, user.Password, user.Role);
    }
}


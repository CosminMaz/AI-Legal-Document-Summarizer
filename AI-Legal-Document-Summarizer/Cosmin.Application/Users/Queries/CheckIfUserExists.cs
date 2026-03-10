using Cosmin.Application.Abstractions;
using MediatR;

namespace Cosmin.Application.Users.Queries;

public record CheckIfUserExistsQuery(string Email) : IRequest<bool>;

public sealed class CheckIfUserExistsHandler : IRequestHandler<CheckIfUserExistsQuery, bool>
{
    private readonly IUserRepository _repository;

    public CheckIfUserExistsHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public Task<bool> Handle(CheckIfUserExistsQuery request, CancellationToken cancellationToken)
    {
        var exists = _repository.Query().Any(u => u.Email == request.Email);
        return Task.FromResult(exists);
    }
}
    
 
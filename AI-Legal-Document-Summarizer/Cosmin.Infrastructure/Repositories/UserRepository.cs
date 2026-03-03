using Cosmin.Application.Abstractions;
using Cosmin.Domain.Entities;
using Cosmin.Infrastructure.Persistence;

namespace Cosmin.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{   
    private readonly AppDbContext _context = context;

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public IQueryable<User> Query()
    {
        throw new NotImplementedException();
    }
}
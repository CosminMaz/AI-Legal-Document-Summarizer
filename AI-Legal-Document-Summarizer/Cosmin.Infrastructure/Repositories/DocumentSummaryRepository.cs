using Cosmin.Application.Abstractions;
using Cosmin.Domain.Entities;
using Cosmin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cosmin.Infrastructure.Repositories;

public sealed class DocumentSummaryRepository(AppDbContext dbContext) : IDocumentSummaryRepository
{
    public async Task AddAsync(SavedSummary summary, CancellationToken cancellationToken = default)
    {
        await dbContext.DocumentSummaries.AddAsync(summary, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SavedSummary>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.DocumentSummaries
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

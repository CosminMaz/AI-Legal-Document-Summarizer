using Cosmin.Domain.Entities;

namespace Cosmin.Application.Abstractions;

public interface IDocumentSummaryRepository
{
    Task AddAsync(SavedSummary summary, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SavedSummary>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

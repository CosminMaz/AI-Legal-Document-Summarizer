using Cosmin.Application.Abstractions;
using MediatR;

namespace Cosmin.Application.Documents.Queries;

public sealed class GetSummariesQueryHandler(IDocumentSummaryRepository documentSummaryRepository)
    : IRequestHandler<GetSummariesQuery, IReadOnlyList<SummaryDto>>
{
    public async Task<IReadOnlyList<SummaryDto>> Handle(GetSummariesQuery request, CancellationToken cancellationToken)
    {
        var summaries = await documentSummaryRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        return summaries
            .Select(s => new SummaryDto(s.Id, s.DocumentTitle, s.Summary, s.Model, s.CreatedAt))
            .ToList();
    }
}

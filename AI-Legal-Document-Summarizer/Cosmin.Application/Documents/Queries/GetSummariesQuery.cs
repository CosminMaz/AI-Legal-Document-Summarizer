using MediatR;

namespace Cosmin.Application.Documents.Queries;

public sealed record GetSummariesQuery(Guid UserId) : IRequest<IReadOnlyList<SummaryDto>>;

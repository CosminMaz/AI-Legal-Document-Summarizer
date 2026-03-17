namespace Cosmin.Application.Documents.Queries;

public sealed record SummaryDto(
    Guid Id,
    string? DocumentTitle,
    string Summary,
    string Model,
    DateTime CreatedAt);

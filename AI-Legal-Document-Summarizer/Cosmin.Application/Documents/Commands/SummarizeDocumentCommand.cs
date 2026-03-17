using MediatR;

namespace Cosmin.Application.Documents.Commands;

public sealed record SummarizeDocumentCommand(
    byte[] FileContent,
    string FileName,
    string ContentType,
    string? DocumentTitle,
    int MaxSummaryWords = 200,
    Guid? UserId = null) : IRequest<SummarizeDocumentResponse>;

public sealed record SummarizeDocumentResponse(string Summary, string Model);

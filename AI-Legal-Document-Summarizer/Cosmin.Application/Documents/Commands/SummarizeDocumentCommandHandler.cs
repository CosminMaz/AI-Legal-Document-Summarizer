using Cosmin.Application.Abstractions;
using Cosmin.Domain.Entities;
using MediatR;

namespace Cosmin.Application.Documents.Commands;

public sealed class SummarizeDocumentCommandHandler(
    IAiSummarizerService aiSummarizerService,
    IDocumentSummaryRepository documentSummaryRepository)
    : IRequestHandler<SummarizeDocumentCommand, SummarizeDocumentResponse>
{
    public async Task<SummarizeDocumentResponse> Handle(
        SummarizeDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var result = await aiSummarizerService.SummarizeAsync(
            request.FileContent,
            request.FileName,
            request.ContentType,
            request.DocumentTitle,
            request.MaxSummaryWords,
            cancellationToken);

        if (request.UserId.HasValue)
        {
            var saved = SavedSummary.Create(request.UserId.Value, request.DocumentTitle, result.Summary, result.Model);
            await documentSummaryRepository.AddAsync(saved, cancellationToken);
        }

        return new SummarizeDocumentResponse(result.Summary, result.Model);
    }
}

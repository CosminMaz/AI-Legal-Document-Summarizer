using Cosmin.Domain.Entities;

namespace Cosmin.Application.Abstractions;

public interface IAiSummarizerService
{
    Task<DocumentSummary> SummarizeAsync(
        byte[] fileContent,
        string fileName,
        string contentType,
        string? documentTitle,
        int maxSummaryWords,
        CancellationToken cancellationToken);
}

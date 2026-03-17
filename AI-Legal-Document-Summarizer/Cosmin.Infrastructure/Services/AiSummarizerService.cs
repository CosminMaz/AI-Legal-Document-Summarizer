using System.Net.Http.Headers;
using System.Text.Json;
using Cosmin.Application.Abstractions;
using Cosmin.Domain.Entities;

namespace Cosmin.Infrastructure.Services;

public sealed class AiSummarizerService(IHttpClientFactory httpClientFactory) : IAiSummarizerService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<DocumentSummary> SummarizeAsync(
        byte[] fileContent,
        string fileName,
        string contentType,
        string? documentTitle,
        int maxSummaryWords,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("AiSummarizer");

        using var form = new MultipartFormDataContent();

        var fileBytes = new ByteArrayContent(fileContent);
        fileBytes.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        form.Add(fileBytes, "file", fileName);

        if (documentTitle is not null)
            form.Add(new StringContent(documentTitle), "document_title");

        form.Add(new StringContent(maxSummaryWords.ToString()), "max_summary_words");

        var response = await client.PostAsync("/summarize", form, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<SummaryResponse>(json, JsonOptions)!;

        return new DocumentSummary(result.Summary, result.Model);
    }

    private sealed record SummaryResponse(string Summary, string Model);
}

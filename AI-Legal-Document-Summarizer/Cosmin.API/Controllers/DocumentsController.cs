using Cosmin.Application.Documents.Commands;
using Cosmin.Application.Documents.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cosmin.API.Controllers;

[ApiController]
[Route("/api/v1/documents/")]
public class DocumentsController(IMediator mediator) : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = [".pdf", ".docx", ".txt", ".md", ".rtf"];
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    [HttpPost("summarize")]
    [ProducesResponseType(typeof(SummarizeDocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> SummarizeDocument(
        IFormFile file,
        [FromForm] string? documentTitle = null,
        [FromForm] int maxSummaryWords = 200,
        [FromForm] Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return BadRequest(new { message = $"Unsupported format. Allowed: {string.Join(", ", AllowedExtensions)}" });

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(new { message = "File exceeds the 10 MB limit." });

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, cancellationToken);

        var command = new SummarizeDocumentCommand(
            ms.ToArray(),
            file.FileName,
            file.ContentType,
            documentTitle,
            maxSummaryWords,
            userId);

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("summaries/{userId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<SummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummaries(Guid userId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSummariesQuery(userId), cancellationToken);
        return Ok(result);
    }
}

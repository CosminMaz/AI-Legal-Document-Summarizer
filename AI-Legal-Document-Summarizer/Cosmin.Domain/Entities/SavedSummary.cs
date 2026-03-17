namespace Cosmin.Domain.Entities;

public class SavedSummary
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string? DocumentTitle { get; private set; }
    public string Summary { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private SavedSummary() { } // EF Core

    public static SavedSummary Create(Guid userId, string? documentTitle, string summary, string model)
        => new SavedSummary
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DocumentTitle = documentTitle,
            Summary = summary,
            Model = model,
            CreatedAt = DateTime.UtcNow
        };
}

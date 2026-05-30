namespace DigitalSignature.Domain.Entities;

public sealed class AuditEvent
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid? DocumentId { get; private set; }
    public string Action { get; private set; }
    public string? Details { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private AuditEvent() { Action = string.Empty; }

    public static AuditEvent Create(
        string action,
        Guid? userId = null,
        Guid? documentId = null,
        string? details = null,
        DateTimeOffset? occurredAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);

        return new AuditEvent
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DocumentId = documentId,
            Action = action,
            Details = details,
            OccurredAt = occurredAt ?? DateTimeOffset.UtcNow
        };
    }
}

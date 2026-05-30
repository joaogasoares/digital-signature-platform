using DigitalSignature.Domain.ValueObjects;

namespace DigitalSignature.Domain.Entities;

public sealed class Document
{
    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    public string FileName { get; private set; }
    public string ContentType { get; private set; }
    public long FileSizeBytes { get; private set; }
    public string StoragePath { get; private set; }
    public DocumentHash Hash { get; private set; }
    public DocumentStatus Status { get; private set; }
    public DateTimeOffset UploadedAt { get; private set; }

    private Document()
    {
        FileName = string.Empty;
        ContentType = string.Empty;
        StoragePath = string.Empty;
        Hash = null!;
    }

    public static Document Create(
        Guid ownerId,
        string fileName,
        string contentType,
        long fileSizeBytes,
        string storagePath,
        DocumentHash hash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);
        ArgumentNullException.ThrowIfNull(hash);

        if (fileSizeBytes <= 0)
            throw new ArgumentException("File size must be positive.", nameof(fileSizeBytes));

        return new Document
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            FileName = fileName,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            StoragePath = storagePath,
            Hash = hash,
            Status = DocumentStatus.Uploaded,
            UploadedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkAsSigned() => Status = DocumentStatus.Signed;
}

public enum DocumentStatus
{
    Uploaded,
    Signed
}

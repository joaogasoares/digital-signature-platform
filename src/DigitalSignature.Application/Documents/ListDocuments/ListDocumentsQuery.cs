using DigitalSignature.Application.Common.Models;
using MediatR;

namespace DigitalSignature.Application.Documents.ListDocuments;

public sealed record ListDocumentsQuery(Guid OwnerId) : IRequest<Result<IReadOnlyList<DocumentSummary>>>;

public sealed record DocumentSummary(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string Status,
    DateTimeOffset UploadedAt);

using DigitalSignature.Application.Common.Models;
using MediatR;

namespace DigitalSignature.Application.Documents.GetDocument;

public sealed record GetDocumentQuery(Guid DocumentId, Guid RequesterId) : IRequest<Result<DocumentResponse>>;

public sealed record DocumentResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string HashValue,
    string Status,
    DateTimeOffset UploadedAt,
    byte[] Content);

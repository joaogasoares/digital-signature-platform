using DigitalSignature.Application.Common.Models;
using MediatR;

namespace DigitalSignature.Application.Documents.UploadDocument;

public sealed record UploadDocumentCommand(
    Guid OwnerId,
    string FileName,
    string ContentType,
    Stream FileContent,
    long FileSizeBytes) : IRequest<Result<Guid>>;

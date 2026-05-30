using DigitalSignature.Application.Common.Interfaces;
using DigitalSignature.Application.Common.Models;
using MediatR;

namespace DigitalSignature.Application.Documents.ListDocuments;

internal sealed class ListDocumentsHandler(IDocumentRepository documentRepository)
    : IRequestHandler<ListDocumentsQuery, Result<IReadOnlyList<DocumentSummary>>>
{
    public async Task<Result<IReadOnlyList<DocumentSummary>>> Handle(
        ListDocumentsQuery request, CancellationToken cancellationToken)
    {
        var docs = await documentRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);

        var summaries = docs
            .Select(d => new DocumentSummary(d.Id, d.FileName, d.ContentType, d.FileSizeBytes,
                d.Status.ToString(), d.UploadedAt))
            .ToList();

        return Result<IReadOnlyList<DocumentSummary>>.Success(summaries);
    }
}

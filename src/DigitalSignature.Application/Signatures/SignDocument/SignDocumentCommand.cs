using DigitalSignature.Application.Common.Models;
using MediatR;

namespace DigitalSignature.Application.Signatures.SignDocument;

public sealed record SignDocumentCommand(Guid DocumentId, Guid SignerId) : IRequest<Result<Guid>>;

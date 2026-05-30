using DigitalSignature.Application.Common.Interfaces;
using DigitalSignature.Application.Signatures.ValidateSignature;
using DigitalSignature.Domain.Entities;
using DigitalSignature.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace DigitalSignature.Application.UnitTests.Signatures;

public class ValidateSignatureHandlerTests
{
    private readonly IDocumentRepository _docRepo = Substitute.For<IDocumentRepository>();
    private readonly ISignatureRepository _sigRepo = Substitute.For<ISignatureRepository>();
    private readonly ISignatureService _sigSvc = Substitute.For<ISignatureService>();

    private ValidateSignatureHandler CreateHandler() =>
        new(_docRepo, _sigRepo, _sigSvc);

    private static Document CreateDocument()
        => Document.Create(Guid.NewGuid(), "doc.pdf", "application/pdf", 100,
            "path", DocumentHash.FromSha256(new string('a', 64)));

    private static Domain.Entities.Signature CreateSignature(Guid docId)
        => Domain.Entities.Signature.Create(docId, Guid.NewGuid(), new byte[256], "RSA-PSS", "thumb", DateTimeOffset.UtcNow);

    [Fact]
    public async Task Handle_valid_signature_returns_isValid_true()
    {
        var doc = CreateDocument();
        var sig = CreateSignature(doc.Id);
        _docRepo.GetByIdAsync(doc.Id).Returns(doc);
        _sigRepo.GetByDocumentIdAsync(doc.Id).Returns(sig);
        _sigSvc.Verify(Arg.Any<byte[]>(), sig.SignatureBytes).Returns(true);

        var result = await CreateHandler().Handle(new ValidateSignatureQuery(doc.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_tampered_document_returns_isValid_false()
    {
        var doc = CreateDocument();
        var sig = CreateSignature(doc.Id);
        _docRepo.GetByIdAsync(doc.Id).Returns(doc);
        _sigRepo.GetByDocumentIdAsync(doc.Id).Returns(sig);
        _sigSvc.Verify(Arg.Any<byte[]>(), sig.SignatureBytes).Returns(false);

        var result = await CreateHandler().Handle(new ValidateSignatureQuery(doc.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsValid.Should().BeFalse();
        result.Value.FailureReason.Should().Contain("tampered");
    }

    [Fact]
    public async Task Handle_unsigned_document_returns_isValid_false_with_reason()
    {
        var doc = CreateDocument();
        _docRepo.GetByIdAsync(doc.Id).Returns(doc);
        _sigRepo.GetByDocumentIdAsync(doc.Id).Returns((Domain.Entities.Signature?)null);

        var result = await CreateHandler().Handle(new ValidateSignatureQuery(doc.Id), CancellationToken.None);

        result.Value!.IsValid.Should().BeFalse();
        result.Value.FailureReason.Should().Contain("not been signed");
    }
}

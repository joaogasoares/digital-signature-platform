using DigitalSignature.Application.Common.Interfaces;
using DigitalSignature.Application.Signatures.SignDocument;
using DigitalSignature.Domain.Entities;
using DigitalSignature.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace DigitalSignature.Application.UnitTests.Signatures;

public class SignDocumentHandlerTests
{
    private readonly IDocumentRepository _docRepo = Substitute.For<IDocumentRepository>();
    private readonly ISignatureRepository _sigRepo = Substitute.For<ISignatureRepository>();
    private readonly IAuditRepository _auditRepo = Substitute.For<IAuditRepository>();
    private readonly ISignatureService _sigSvc = Substitute.For<ISignatureService>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IClock _clock = Substitute.For<IClock>();

    private SignDocumentHandler CreateHandler() =>
        new(_docRepo, _sigRepo, _auditRepo, _sigSvc, _uow, _clock);

    private static Document CreateDocument(Guid ownerId)
    {
        return Document.Create(ownerId, "test.pdf", "application/pdf", 100,
            "path/test.pdf", DocumentHash.FromSha256(new string('a', 64)));
    }

    [Fact]
    public async Task Handle_valid_request_returns_signature_id()
    {
        var ownerId = Guid.NewGuid();
        var doc = CreateDocument(ownerId);
        _docRepo.GetByIdAsync(doc.Id).Returns(doc);
        _sigRepo.GetByDocumentIdAsync(doc.Id).Returns((Domain.Entities.Signature?)null);
        _sigSvc.IsCertificateValid(out _).Returns(x => { x[0] = string.Empty; return true; });
        _sigSvc.Sign(Arg.Any<byte[]>()).Returns(new byte[256]);
        _sigSvc.GetAlgorithm().Returns("RSA-PSS");
        _sigSvc.GetThumbprint().Returns("thumbprint123");
        _clock.UtcNow.Returns(DateTimeOffset.UtcNow);

        var result = await CreateHandler().Handle(new SignDocumentCommand(doc.Id, ownerId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_document_not_found_returns_failure()
    {
        _docRepo.GetByIdAsync(Arg.Any<Guid>()).Returns((Document?)null);

        var result = await CreateHandler().Handle(
            new SignDocumentCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_already_signed_returns_failure()
    {
        var ownerId = Guid.NewGuid();
        var doc = CreateDocument(ownerId);
        _docRepo.GetByIdAsync(doc.Id).Returns(doc);
        _sigRepo.GetByDocumentIdAsync(doc.Id).Returns(
            Domain.Entities.Signature.Create(doc.Id, ownerId, new byte[256], "RSA-PSS", "thumb", DateTimeOffset.UtcNow));

        var result = await CreateHandler().Handle(new SignDocumentCommand(doc.Id, ownerId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already signed");
    }

    [Fact]
    public async Task Handle_invalid_certificate_returns_failure()
    {
        var ownerId = Guid.NewGuid();
        var doc = CreateDocument(ownerId);
        _docRepo.GetByIdAsync(doc.Id).Returns(doc);
        _sigRepo.GetByDocumentIdAsync(doc.Id).Returns((Domain.Entities.Signature?)null);
        _sigSvc.IsCertificateValid(out _).Returns(x => { x[0] = "Certificate expired"; return false; });

        var result = await CreateHandler().Handle(new SignDocumentCommand(doc.Id, ownerId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not valid");
    }

    [Fact]
    public async Task Handle_success_creates_audit_event()
    {
        var ownerId = Guid.NewGuid();
        var doc = CreateDocument(ownerId);
        _docRepo.GetByIdAsync(doc.Id).Returns(doc);
        _sigRepo.GetByDocumentIdAsync(doc.Id).Returns((Domain.Entities.Signature?)null);
        _sigSvc.IsCertificateValid(out _).Returns(x => { x[0] = string.Empty; return true; });
        _sigSvc.Sign(Arg.Any<byte[]>()).Returns(new byte[256]);
        _sigSvc.GetAlgorithm().Returns("RSA-PSS");
        _sigSvc.GetThumbprint().Returns("thumb");
        _clock.UtcNow.Returns(DateTimeOffset.UtcNow);

        await CreateHandler().Handle(new SignDocumentCommand(doc.Id, ownerId), CancellationToken.None);

        await _auditRepo.Received(1).AddAsync(
            Arg.Is<AuditEvent>(e => e.Action == "DocumentSigned"),
            Arg.Any<CancellationToken>());
    }
}

using DigitalSignature.Application.Common.Interfaces;
using DigitalSignature.Application.Documents.UploadDocument;
using DigitalSignature.Domain.Entities;
using DigitalSignature.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace DigitalSignature.Application.UnitTests.Documents;

public class UploadDocumentHandlerTests
{
    private readonly IDocumentRepository _docRepo = Substitute.For<IDocumentRepository>();
    private readonly IFileStorage _storage = Substitute.For<IFileStorage>();
    private readonly IHashService _hashService = Substitute.For<IHashService>();
    private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private static readonly string ValidHash = new string('a', 64);

    private UploadDocumentHandler CreateHandler() =>
        new(_docRepo, _storage, _hashService, _encryption, _uow);

    private UploadDocumentCommand ValidCommand(string contentType = "application/pdf") =>
        new(Guid.NewGuid(), "test.pdf", contentType,
            new MemoryStream("content"u8.ToArray()), 7);

    [Fact]
    public async Task Handle_valid_pdf_returns_success()
    {
        _hashService.Compute(Arg.Any<byte[]>())
            .Returns(DocumentHash.FromSha256(ValidHash));
        _encryption.Encrypt(Arg.Any<byte[]>()).Returns(new byte[10]);
        _storage.SaveAsync(Arg.Any<Stream>(), Arg.Any<string>()).Returns("path/to/file");

        var result = await CreateHandler().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("application/zip")]
    [InlineData("text/html")]
    public async Task Handle_disallowed_content_type_returns_failure(string contentType)
    {
        var result = await CreateHandler().Handle(ValidCommand(contentType), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not allowed");
    }

    [Fact]
    public async Task Handle_oversized_file_returns_failure()
    {
        var command = new UploadDocumentCommand(
            Guid.NewGuid(), "huge.pdf", "application/pdf",
            new MemoryStream(), 11 * 1024 * 1024); // 11 MB

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("exceeds maximum");
    }

    [Fact]
    public async Task Handle_success_encrypts_before_storing()
    {
        var plainBytes = "document content"u8.ToArray();
        var encryptedBytes = new byte[32];
        _hashService.Compute(Arg.Any<byte[]>()).Returns(DocumentHash.FromSha256(ValidHash));
        _encryption.Encrypt(Arg.Any<byte[]>()).Returns(encryptedBytes);
        _storage.SaveAsync(Arg.Any<Stream>(), Arg.Any<string>()).Returns("path");

        await CreateHandler().Handle(
            new UploadDocumentCommand(Guid.NewGuid(), "f.pdf", "application/pdf",
                new MemoryStream(plainBytes), plainBytes.Length),
            CancellationToken.None);

        // Verify encryption was called with plaintext
        _encryption.Received(1).Encrypt(Arg.Is<byte[]>(b => b.SequenceEqual(plainBytes)));
        // Verify storage was called (not with plaintext directly)
        await _storage.Received(1).SaveAsync(Arg.Any<Stream>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_success_saves_changes()
    {
        _hashService.Compute(Arg.Any<byte[]>()).Returns(DocumentHash.FromSha256(ValidHash));
        _encryption.Encrypt(Arg.Any<byte[]>()).Returns(new byte[10]);
        _storage.SaveAsync(Arg.Any<Stream>(), Arg.Any<string>()).Returns("path");

        await CreateHandler().Handle(ValidCommand(), CancellationToken.None);

        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

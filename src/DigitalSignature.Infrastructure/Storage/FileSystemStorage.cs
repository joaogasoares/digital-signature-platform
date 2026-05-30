using DigitalSignature.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DigitalSignature.Infrastructure.Storage;

internal sealed class FileSystemStorage : IFileStorage
{
    private readonly string _basePath;

    public FileSystemStorage(IConfiguration configuration)
    {
        _basePath = configuration["Storage:BasePath"]
            ?? Path.Combine(Path.GetTempPath(), "digital-signature-storage");

        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        var relativePath = Path.Combine(
            DateTime.UtcNow.ToString("yyyy/MM/dd"),
            $"{Guid.NewGuid()}{Path.GetExtension(fileName)}");

        var fullPath = Path.Combine(_basePath, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return relativePath;
    }

    public Task<Stream> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, path);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {path}");

        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, path);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }
}

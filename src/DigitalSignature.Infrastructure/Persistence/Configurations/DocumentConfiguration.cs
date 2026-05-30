using DigitalSignature.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalSignature.Infrastructure.Persistence.Configurations;

internal sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.OwnerId).IsRequired();
        builder.HasIndex(d => d.OwnerId);

        builder.Property(d => d.FileName).IsRequired().HasMaxLength(512);
        builder.Property(d => d.ContentType).IsRequired().HasMaxLength(128);
        builder.Property(d => d.FileSizeBytes).IsRequired();
        builder.Property(d => d.StoragePath).IsRequired().HasMaxLength(1024);
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(d => d.UploadedAt).IsRequired();

        builder.OwnsOne(d => d.Hash, hash =>
        {
            hash.Property(h => h.Value).HasColumnName("hash_value").HasMaxLength(128).IsRequired();
            hash.Property(h => h.Algorithm).HasColumnName("hash_algorithm").HasMaxLength(20).IsRequired();
        });
    }
}

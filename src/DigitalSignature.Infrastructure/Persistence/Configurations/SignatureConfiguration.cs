using DigitalSignature.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalSignature.Infrastructure.Persistence.Configurations;

internal sealed class SignatureConfiguration : IEntityTypeConfiguration<Signature>
{
    public void Configure(EntityTypeBuilder<Signature> builder)
    {
        builder.ToTable("signatures");
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.DocumentId).IsUnique();
        builder.Property(s => s.SignatureBytes).IsRequired();
        builder.Property(s => s.Algorithm).IsRequired().HasMaxLength(50);
        builder.Property(s => s.CertificateThumbprint).IsRequired().HasMaxLength(128);
        builder.Property(s => s.SignedAt).IsRequired();
    }
}

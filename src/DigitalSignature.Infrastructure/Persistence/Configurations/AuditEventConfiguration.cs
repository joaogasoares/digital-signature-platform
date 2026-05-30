using DigitalSignature.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalSignature.Infrastructure.Persistence.Configurations;

internal sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("audit_events");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.DocumentId);
        builder.HasIndex(e => e.UserId);
        builder.Property(e => e.Action).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Details).HasMaxLength(500);
        builder.Property(e => e.OccurredAt).IsRequired();
        builder.ToTable(t => t.HasCheckConstraint("ck_audit_events_immutable", "1=1")); // logical append-only enforced at app level
    }
}

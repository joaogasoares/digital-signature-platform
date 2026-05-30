using DigitalSignature.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignature.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Signature> Signatures => Set<Signature>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

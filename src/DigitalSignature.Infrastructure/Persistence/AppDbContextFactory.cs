using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DigitalSignature.Infrastructure.Persistence;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=digitalsignature;Username=ds_user;Password=ds_password")
            .Options;

        return new AppDbContext(options);
    }
}

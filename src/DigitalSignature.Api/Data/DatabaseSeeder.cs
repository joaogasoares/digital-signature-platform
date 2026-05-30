using DigitalSignature.Application.Common.Interfaces;
using DigitalSignature.Domain.Entities;
using DigitalSignature.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignature.Api.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            await context.Database.MigrateAsync();

            if (!await context.Users.AnyAsync())
            {
                var demo = User.Create("demo@digitalsignature.com", hasher.Hash("Demo@123456"));
                var admin = User.Create("admin@digitalsignature.com", hasher.Hash("Admin@123456"), "Admin");

                context.Users.AddRange(demo, admin);
                await context.SaveChangesAsync();

                logger.LogInformation("Seed: created demo and admin users.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Seed failed.");
        }
    }
}

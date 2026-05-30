using System.Text;
using DigitalSignature.Application.Common.Interfaces;
using DigitalSignature.Infrastructure.Auth;
using DigitalSignature.Infrastructure.Common;
using DigitalSignature.Infrastructure.Cryptography;
using DigitalSignature.Infrastructure.Persistence;
using DigitalSignature.Infrastructure.Persistence.Repositories;
using DigitalSignature.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DigitalSignature.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IHashService, Sha256HashService>();
        services.AddSingleton<IEncryptionService, AesEncryptionService>();
        services.AddSingleton<IFileStorage, FileSystemStorage>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        var jwtSecret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is required.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? "digital-signature-platform",
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"] ?? "digital-signature-platform",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }
}

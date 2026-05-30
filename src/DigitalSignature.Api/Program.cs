using DigitalSignature.Api.Data;
using DigitalSignature.Api.Endpoints;
using DigitalSignature.Api.Middleware;
using DigitalSignature.Application;
using DigitalSignature.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddMediatR(DigitalSignature.Application.DependencyInjection.Assembly);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Seed demo data in production on startup (idempotent)
if (!app.Environment.IsEnvironment("Testing"))
{
    await DatabaseSeeder.SeedAsync(app.Services);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("HealthCheck");

app.MapUsersEndpoints();
app.MapDocumentsEndpoints();
app.MapSignaturesEndpoints();
app.MapAdminEndpoints();

app.Run();

public partial class Program { }

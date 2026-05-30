using DigitalSignature.Application.Audit.GetAuditTrail;
using MediatR;

namespace DigitalSignature.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapGet("/audit", async (Guid? documentId, Guid? userId, ISender sender) =>
        {
            var result = await sender.Send(new GetAuditTrailQuery(documentId, userId));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        return app;
    }
}

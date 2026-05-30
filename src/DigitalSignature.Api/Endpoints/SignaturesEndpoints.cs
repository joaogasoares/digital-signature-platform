using System.Security.Claims;
using DigitalSignature.Application.Audit.GetAuditTrail;
using DigitalSignature.Application.Signatures.SignDocument;
using DigitalSignature.Application.Signatures.ValidateSignature;
using MediatR;

namespace DigitalSignature.Api.Endpoints;

public static class SignaturesEndpoints
{
    public static IEndpointRouteBuilder MapSignaturesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").WithTags("Signatures");

        group.MapPost("/documents/{id:guid}/sign", async (Guid id, ClaimsPrincipal user, ISender sender) =>
        {
            var signerId = GetUserId(user);
            var result = await sender.Send(new SignDocumentCommand(id, signerId));
            return result.IsSuccess
                ? Results.Ok(new { signatureId = result.Value })
                : Results.BadRequest(new { error = result.Error });
        }).RequireAuthorization();

        group.MapGet("/documents/{id:guid}/validate", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new ValidateSignatureQuery(id));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        });

        group.MapGet("/audit", async (Guid? documentId, Guid? userId, ISender sender) =>
        {
            var result = await sender.Send(new GetAuditTrailQuery(documentId, userId));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();

        return app;
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("User ID claim not found.");
        return Guid.Parse(sub);
    }
}

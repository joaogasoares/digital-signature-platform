using System.Security.Claims;
using DigitalSignature.Application.Documents.GetDocument;
using DigitalSignature.Application.Documents.ListDocuments;
using DigitalSignature.Application.Documents.UploadDocument;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace DigitalSignature.Api.Endpoints;

public static class DocumentsEndpoints
{
    public static IEndpointRouteBuilder MapDocumentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/documents").WithTags("Documents").RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, ISender sender) =>
        {
            var ownerId = GetUserId(user);
            var result = await sender.Send(new ListDocumentsQuery(ownerId));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal user, ISender sender) =>
        {
            var requesterId = GetUserId(user);
            var result = await sender.Send(new GetDocumentQuery(id, requesterId));

            if (!result.IsSuccess) return Results.NotFound(result.Error);

            var doc = result.Value!;
            return Results.File(doc.Content, doc.ContentType, doc.FileName);
        });

        group.MapPost("/", [AllowAnonymous] async (IFormFile file, ClaimsPrincipal user, ISender sender) =>
        {
            var ownerId = GetUserId(user);

            await using var stream = file.OpenReadStream();
            var command = new UploadDocumentCommand(
                ownerId, file.FileName, file.ContentType, stream, file.Length);

            var result = await sender.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/documents/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error });
        }).RequireAuthorization().DisableAntiforgery();

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

using DigitalSignature.Application.Users.RegisterUser;
using MediatR;

namespace DigitalSignature.Api.Endpoints;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapPost("/register", async (RegisterUserRequest request, ISender sender) =>
        {
            var command = new RegisterUserCommand(request.Email, request.Password);
            var result = await sender.Send(command);

            return result.IsSuccess
                ? Results.Created($"/api/users/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error });
        });

        return app;
    }
}

public sealed record RegisterUserRequest(string Email, string Password);

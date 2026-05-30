using DigitalSignature.Application.Users.LoginUser;
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
            var result = await sender.Send(new RegisterUserCommand(request.Email, request.Password));

            return result.IsSuccess
                ? Results.Created($"/api/users/{result.Value}", new { id = result.Value })
                : Results.BadRequest(new { error = result.Error });
        });

        group.MapPost("/login", async (LoginUserRequest request, ISender sender) =>
        {
            var result = await sender.Send(new LoginUserCommand(request.Email, request.Password));

            return result.IsSuccess
                ? Results.Ok(new { token = result.Value })
                : Results.Unauthorized();
        });

        return app;
    }
}

public sealed record RegisterUserRequest(string Email, string Password);
public sealed record LoginUserRequest(string Email, string Password);

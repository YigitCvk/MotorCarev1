using Carter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using MotorCare.Application.Auth;
using MotorCare.Application.Auth.Commands.Login;
using MotorCare.Application.Auth.Commands.Logout;
using MotorCare.Application.Auth.Commands.RefreshToken;
using MotorCare.Application.Auth.Queries.GetCurrentUser;

namespace MotorCare.Api.Modules;

public sealed class AuthModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth")
            .WithOpenApi();

        group.MapPost("/login", async (LoginCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("Login")
        .Produces<AuthResponseDto>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/refresh-token", async (RefreshTokenCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("RefreshToken")
        .Produces<AuthResponseDto>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/logout", async (LogoutCommand command, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(command, ct);
            return Results.NoContent();
        })
        .WithName("Logout")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/me", [Authorize] async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetCurrentUserQuery(), ct);
            return Results.Ok(result);
        })
        .WithName("GetCurrentUser")
        .Produces<CurrentUserDto>()
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}

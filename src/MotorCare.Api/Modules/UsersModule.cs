using Carter;
using MediatR;
using MotorCare.Api.Authorization;
using MotorCare.Application.Users.Commands.CreateTenantUser;
using MotorCare.Application.Users.Commands.DeactivateUser;
using MotorCare.Application.Users.Commands.UpdateUserRole;
using MotorCare.Application.Users.Queries.GetUsers;
using MotorCare.Domain.Enums;

namespace MotorCare.Api.Modules;

public sealed class UsersModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi();

        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetUsersQuery(), ct);
            return Results.Ok(result);
        })
        .WithName("GetUsers")
        .RequireAuthorization(AuthorizationPolicies.TenantManagement)
        .Produces<IReadOnlyList<UserDto>>()
        .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/", async (CreateUserRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new CreateTenantUserCommand(
                request.FullName,
                request.Email,
                request.Password,
                request.Role);

            var id = await mediator.Send(command, ct);
            return Results.Created($"/api/users/{id}", id);
        })
        .WithName("CreateUser")
        .RequireAuthorization(AuthorizationPolicies.TenantManagement)
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}/role", async (Guid id, UpdateUserRoleRequest request, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new UpdateUserRoleCommand(id, request.Role), ct);
            return Results.NoContent();
        })
        .WithName("UpdateUserRole")
        .RequireAuthorization(AuthorizationPolicies.TenantManagement)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPatch("/{id:guid}/deactivate", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new DeactivateUserCommand(id), ct);
            return Results.NoContent();
        })
        .WithName("DeactivateUser")
        .RequireAuthorization(AuthorizationPolicies.TenantManagement)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }

    public sealed record CreateUserRequest(string FullName, string Email, string Password, UserRole Role);
    public sealed record UpdateUserRoleRequest(UserRole Role);
}

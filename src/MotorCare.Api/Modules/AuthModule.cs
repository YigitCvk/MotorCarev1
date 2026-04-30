using Carter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using MotorCare.Api.Authorization;
using MotorCare.Application.Auth;
using MotorCare.Application.Auth.Commands.ForgotPassword;
using MotorCare.Application.Auth.Commands.Login;
using MotorCare.Application.Auth.Commands.Logout;
using MotorCare.Application.Auth.Commands.ConfirmDisableTwoFactorEmail;
using MotorCare.Application.Auth.Commands.ConfirmEnableTwoFactorEmail;
using MotorCare.Application.Auth.Commands.RefreshToken;
using MotorCare.Application.Auth.Commands.ResendEmailVerification;
using MotorCare.Application.Auth.Commands.ResendTwoFactorEmail;
using MotorCare.Application.Auth.Commands.ResetPassword;
using MotorCare.Application.Auth.Commands.SendDisableTwoFactorEmailCode;
using MotorCare.Application.Auth.Commands.SendEnableTwoFactorEmailCode;
using MotorCare.Application.Auth.Commands.VerifyEmail;
using MotorCare.Application.Auth.Commands.VerifyTwoFactorEmail;
using MotorCare.Application.Auth.Queries.GetCurrentUser;
using MotorCare.Application.Auth.Queries.GetSecurityStatus;
using MotorCare.Application.Tenants.Commands.CreateTenantWithOwner;

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

        group.MapPost("/register", async (CreateTenantWithOwnerCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Created($"/api/tenants/{result.TenantIdentifier}", result);
        })
        .WithName("RegisterTenantWithOwner")
        .Produces<CreateTenantWithOwnerResultDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status409Conflict)
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

        group.MapPost("/forgot-password", async (ForgotPasswordCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("ForgotPassword")
        .Produces<AuthActionMessageDto>()
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/reset-password", async (ResetPasswordCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("ResetPassword")
        .Produces<AuthActionMessageDto>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/verify-email", async (VerifyEmailCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("VerifyEmail")
        .Produces<AuthActionMessageDto>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/resend-email-verification", async (ResendEmailVerificationCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("ResendEmailVerification")
        .Produces<AuthActionMessageDto>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/two-factor/verify", async (VerifyTwoFactorEmailCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("VerifyTwoFactorEmail")
        .Produces<AuthResponseDto>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/two-factor/resend", async (ResendTwoFactorEmailCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("ResendTwoFactorEmail")
        .Produces<AuthActionMessageDto>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/security-status", [Authorize] async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetSecurityStatusQuery(), ct);
            return Results.Ok(result);
        })
        .WithName("GetSecurityStatus")
        .RequireAuthorization()
        .Produces<SecurityStatusDto>()
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/2fa/enable/send-code", [Authorize] async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new SendEnableTwoFactorEmailCodeCommand(), ct);
            return Results.Ok(result);
        })
        .WithName("SendEnableTwoFactorEmailCode")
        .RequireAuthorization()
        .Produces<AuthActionMessageDto>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/2fa/enable/confirm", [Authorize] async (ConfirmEnableTwoFactorEmailCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("ConfirmEnableTwoFactorEmail")
        .RequireAuthorization()
        .Produces<AuthActionMessageDto>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/2fa/disable/send-code", [Authorize] async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new SendDisableTwoFactorEmailCodeCommand(), ct);
            return Results.Ok(result);
        })
        .WithName("SendDisableTwoFactorEmailCode")
        .RequireAuthorization()
        .Produces<AuthActionMessageDto>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/2fa/disable/confirm", [Authorize] async (ConfirmDisableTwoFactorEmailCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithName("ConfirmDisableTwoFactorEmail")
        .RequireAuthorization()
        .Produces<AuthActionMessageDto>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/logout", async (LogoutCommand command, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(command, ct);
            return Results.NoContent();
        })
        .WithName("Logout")
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/me", [Authorize] async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetCurrentUserQuery(), ct);
            return Results.Ok(result);
        })
        .WithName("GetCurrentUser")
        .RequireAuthorization()
        .Produces<CurrentUserDto>()
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}

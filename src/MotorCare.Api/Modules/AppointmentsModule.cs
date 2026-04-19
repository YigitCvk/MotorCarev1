using Carter;
using MediatR;
using MotorCare.Api.Authorization;
using MotorCare.Application.Appointments;
using MotorCare.Application.Appointments.Commands.ConvertAppointmentToServiceOrder;
using MotorCare.Application.Appointments.Commands.CreateAppointment;
using MotorCare.Application.Appointments.Commands.UpdateAppointment;
using MotorCare.Application.Appointments.Commands.UpdateAppointmentStatus;
using MotorCare.Application.Appointments.Queries.GetAppointmentById;
using MotorCare.Application.Appointments.Queries.GetAppointments;
using MotorCare.Application.Common.Models;
using MotorCare.Domain.Enums;

namespace MotorCare.Api.Modules;

public sealed class AppointmentsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments")
            .WithTags("Appointments")
            .WithOpenApi()
            .RequireAuthorization(AuthorizationPolicies.ServiceOrderRead);

        group.MapPost("/", async (CreateAppointmentRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(
                new CreateAppointmentCommand(
                    request.CustomerId,
                    request.VehicleId,
                    request.CustomerName,
                    request.Phone,
                    request.Plate,
                    request.Type,
                    request.StartAt,
                    request.EndAt,
                    request.Note,
                    request.Complaint),
                ct);

            return Results.CreatedAtRoute("GetAppointmentById", new { id = result.Id }, result);
        })
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderWrite)
        .Produces<AppointmentDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/", async (
            DateTimeOffset? startFrom,
            DateTimeOffset? endTo,
            AppointmentStatus? status,
            AppointmentType? type,
            string? q,
            int? pageNumber,
            int? pageSize,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAppointmentsQuery(startFrom, endTo, status, type, q, pageNumber ?? 1, pageSize ?? 20), ct);
            return Results.Ok(result);
        })
        .Produces<PagedResult<AppointmentDto>>()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAppointmentByIdQuery(id), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetAppointmentById")
        .Produces<AppointmentDto>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}", async (Guid id, UpdateAppointmentRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(
                new UpdateAppointmentCommand(
                    id,
                    request.CustomerId,
                    request.VehicleId,
                    request.CustomerName,
                    request.Phone,
                    request.Plate,
                    request.Type,
                    request.StartAt,
                    request.EndAt,
                    request.Note,
                    request.Complaint),
                ct);

            return Results.Ok(result);
        })
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderWrite)
        .Produces<AppointmentDto>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{id:guid}/status", async (Guid id, UpdateAppointmentStatusRequest request, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new UpdateAppointmentStatusCommand(id, request.Status), ct);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderWrite)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:guid}/convert-to-service-order", async (Guid id, ConvertAppointmentToServiceOrderRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var serviceOrderId = await mediator.Send(new ConvertAppointmentToServiceOrderCommand(id, request.VehicleKm), ct);
            return Results.Ok(new { serviceOrderId });
        })
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderWrite)
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new UpdateAppointmentStatusCommand(id, AppointmentStatus.Cancelled), ct);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.ServiceOrderWrite)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }

    public sealed record CreateAppointmentRequest(
        Guid? CustomerId,
        Guid? VehicleId,
        string CustomerName,
        string Phone,
        string? Plate,
        AppointmentType Type,
        DateTimeOffset StartAt,
        DateTimeOffset EndAt,
        string? Note,
        string? Complaint);

    public sealed record UpdateAppointmentRequest(
        Guid? CustomerId,
        Guid? VehicleId,
        string CustomerName,
        string Phone,
        string? Plate,
        AppointmentType Type,
        DateTimeOffset StartAt,
        DateTimeOffset EndAt,
        string? Note,
        string? Complaint);

    public sealed record UpdateAppointmentStatusRequest(AppointmentStatus Status);

    public sealed record ConvertAppointmentToServiceOrderRequest(int VehicleKm);
}

using Carter;
using MediatR;
using MotorCare.Application.PublicRecords;
using MotorCare.Application.PublicRecords.Queries.GetPublicInspectionReport;
using MotorCare.Application.PublicRecords.Queries.GetPublicServiceRecord;

namespace MotorCare.Api.Modules;

public sealed class PublicRecordsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/public")
            .WithTags("PublicRecords")
            .WithOpenApi();

        group.MapGet("/service-record/{slug}", async (
            string slug,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetPublicServiceRecordQuery(slug), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetPublicServiceRecord")
        .AllowAnonymous()
        .Produces<PublicServiceRecordDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/inspection-report/{slug}", async (
            string slug,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetPublicInspectionReportQuery(slug), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetPublicInspectionReport")
        .AllowAnonymous()
        .Produces<PublicInspectionReportDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

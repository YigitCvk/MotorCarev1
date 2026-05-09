using Carter;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.PublicRecords;

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
            IPublicRecordAccessService publicRecordAccessService,
            CancellationToken ct) =>
        {
            var result = await publicRecordAccessService.GetServiceRecordPreviewAsync(slug, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetPublicServiceRecord")
        .AllowAnonymous()
        .Produces<PublicServiceRecordPreviewDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/inspection-report/{slug}", async (
            string slug,
            IPublicRecordAccessService publicRecordAccessService,
            CancellationToken ct) =>
        {
            var result = await publicRecordAccessService.GetInspectionReportPreviewAsync(slug, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetPublicInspectionReport")
        .AllowAnonymous()
        .Produces<PublicInspectionReportPreviewDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

using Carter;
using Microsoft.AspNetCore.Mvc;
using MotorCare.Api.Authorization;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.Imports;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Imports;

namespace MotorCare.Api.Modules;

public sealed class ImportsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/imports")
            .WithTags("Imports")
            .WithOpenApi()
            .RequireAuthorization(AuthorizationPolicies.ImportOperations);

        group.MapGet("/", async (IImportService importService, ICurrentUserProvider user, CancellationToken ct) =>
        {
            var tenantId = user.GetTenantId() ?? string.Empty;
            var batches = await importService.GetBatchesAsync(tenantId, ct);
            return Results.Ok(batches);
        })
        .WithName("GetImportBatches")
        .Produces<IReadOnlyList<ImportBatchDto>>();

        group.MapGet("/{batchId:guid}", async (
            Guid batchId,
            [FromQuery] int previewRows,
            IImportService importService,
            ICurrentUserProvider user,
            CancellationToken ct) =>
        {
            var tenantId = user.GetTenantId() ?? string.Empty;
            var batch = await importService.GetBatchAsync(batchId, tenantId, previewRows > 0 ? previewRows : 50, ct);
            return batch is null ? Results.NotFound() : Results.Ok(batch);
        })
        .WithName("GetImportBatch")
        .Produces<ImportBatchDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{batchId:guid}/rows", async (
            Guid batchId,
            [FromQuery] string? status,
            [FromQuery] int maxRows,
            IImportService importService,
            ICurrentUserProvider user,
            CancellationToken ct) =>
        {
            var tenantId = user.GetTenantId() ?? string.Empty;
            ImportRowStatus? rowStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ImportRowStatus>(status, ignoreCase: true, out var parsed))
                rowStatus = parsed;
            var rows = await importService.GetRowsAsync(batchId, tenantId, rowStatus, maxRows > 0 ? maxRows : 200, ct);
            return Results.Ok(rows);
        })
        .WithName("GetImportBatchRows")
        .Produces<IReadOnlyList<ImportBatchRowDto>>();

        group.MapPost("/upload", async (
            HttpRequest request,
            [FromQuery] string importType,
            IImportService importService,
            ICurrentUserProvider user,
            CancellationToken ct) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest("Multipart form upload required.");

            if (!Enum.TryParse<ImportType>(importType, ignoreCase: true, out var parsedType))
                return Results.BadRequest($"Invalid importType: '{importType}'. Valid values: Customers, Vehicles, ServiceHistory.");

            var form = await request.ReadFormAsync(ct);
            var file = form.Files.GetFile("file");
            if (file is null || file.Length == 0)
                return Results.BadRequest("No file provided.");

            var tenantId = user.GetTenantId() ?? string.Empty;
            var userId = user.GetUserId();

            await using var stream = file.OpenReadStream();
            try
            {
                var batch = await importService.UploadAndParseAsync(
                    stream, file.FileName, file.ContentType, parsedType, tenantId, userId, ct);
                return Results.Ok(batch);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("UploadImport")
        .Accepts<IFormFile>("multipart/form-data")
        .Produces<ImportBatchDto>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .DisableAntiforgery();

        group.MapPost("/{batchId:guid}/commit", async (
            Guid batchId,
            IImportService importService,
            ICurrentUserProvider user,
            CancellationToken ct) =>
        {
            var tenantId = user.GetTenantId() ?? string.Empty;
            try
            {
                var batch = await importService.CommitAsync(batchId, tenantId, ct);
                return Results.Ok(batch);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("CommitImport")
        .Produces<ImportBatchDto>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/templates/{type}", (string type, IImportService importService) =>
        {
            if (!Enum.TryParse<ImportType>(type, ignoreCase: true, out var parsedType))
                return Results.BadRequest($"Invalid import type: '{type}'.");

            var (content, fileName, contentType) = importService.GetCsvTemplate(parsedType);
            return Results.File(content, contentType, fileName);
        })
        .WithName("DownloadImportTemplate")
        .Produces<FileResult>()
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}

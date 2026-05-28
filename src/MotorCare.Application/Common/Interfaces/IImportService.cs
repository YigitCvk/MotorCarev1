using MotorCare.Application.Imports;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.Common.Interfaces;

public interface IImportService
{
    Task<ImportBatchDto> UploadAndParseAsync(
        Stream fileStream,
        string originalFileName,
        string contentType,
        ImportType importType,
        string tenantId,
        Guid? userId,
        CancellationToken cancellationToken = default);

    Task<ImportBatchDto?> GetBatchAsync(
        Guid batchId,
        string tenantId,
        int previewRows = 50,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ImportBatchDto>> GetBatchesAsync(
        string tenantId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ImportBatchRowDto>> GetRowsAsync(
        Guid batchId,
        string tenantId,
        ImportRowStatus? status = null,
        int maxRows = 200,
        CancellationToken cancellationToken = default);

    Task<ImportBatchDto> CommitAsync(
        Guid batchId,
        string tenantId,
        CancellationToken cancellationToken = default);

    (byte[] Content, string FileName, string ContentType) GetCsvTemplate(ImportType importType);
}

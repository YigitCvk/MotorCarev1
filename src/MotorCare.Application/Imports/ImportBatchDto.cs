using MotorCare.Domain.Enums;

namespace MotorCare.Application.Imports;

public sealed record ImportBatchDto(
    Guid Id,
    string TenantId,
    ImportType ImportType,
    string OriginalFileName,
    ImportBatchStatus Status,
    int TotalRows,
    int ValidRows,
    int WarningRows,
    int ErrorRows,
    int ImportedRows,
    int SkippedRows,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    IReadOnlyList<ImportBatchRowDto> PreviewRows);

public sealed record ImportBatchRowDto(
    Guid Id,
    int RowNumber,
    ImportRowStatus Status,
    string RawJson,
    string? NormalizedJson,
    string? ErrorMessage,
    string? WarningMessage);

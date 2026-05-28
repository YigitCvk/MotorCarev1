using MotorCare.Domain.Enums;

namespace MotorCare.Domain.Imports;

public class ImportBatchRow
{
    public Guid Id { get; private set; }
    public Guid ImportBatchId { get; private set; }
    public string TenantId { get; private set; } = string.Empty;
    public int RowNumber { get; private set; }
    public string RawJson { get; private set; } = string.Empty;
    public string? NormalizedJson { get; private set; }
    public ImportRowStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? WarningMessage { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? ImportedAtUtc { get; private set; }

    private ImportBatchRow() { }

    public ImportBatchRow(Guid importBatchId, string tenantId, int rowNumber, string rawJson)
    {
        Id = Guid.NewGuid();
        ImportBatchId = importBatchId;
        TenantId = tenantId;
        RowNumber = rowNumber;
        RawJson = rawJson;
        Status = ImportRowStatus.Valid;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetValid(string? normalizedJson = null)
    {
        Status = ImportRowStatus.Valid;
        NormalizedJson = normalizedJson;
        ErrorMessage = null;
    }

    public void SetWarning(string warningMessage, string? normalizedJson = null)
    {
        Status = ImportRowStatus.Warning;
        WarningMessage = warningMessage;
        NormalizedJson = normalizedJson;
    }

    public void SetError(string errorMessage)
    {
        Status = ImportRowStatus.Error;
        ErrorMessage = errorMessage;
    }

    public void SetImported()
    {
        Status = ImportRowStatus.Imported;
        ImportedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetSkipped(string reason)
    {
        Status = ImportRowStatus.Skipped;
        WarningMessage = reason;
        ImportedAtUtc = DateTimeOffset.UtcNow;
    }
}

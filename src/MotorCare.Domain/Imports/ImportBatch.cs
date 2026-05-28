using MotorCare.Domain.Common;
using MotorCare.Domain.Enums;

namespace MotorCare.Domain.Imports;

public class ImportBatch : ITenantEntity
{
    public Guid Id { get; private set; }
    public string TenantId { get; private set; } = string.Empty;
    public ImportType ImportType { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public ImportBatchStatus Status { get; private set; }
    public int TotalRows { get; private set; }
    public int ValidRows { get; private set; }
    public int WarningRows { get; private set; }
    public int ErrorRows { get; private set; }
    public int ImportedRows { get; private set; }
    public int SkippedRows { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public Guid? CreatedByUserId { get; private set; }

    private readonly List<ImportBatchRow> _rows = [];
    public IReadOnlyCollection<ImportBatchRow> Rows => _rows;

    private ImportBatch() { }

    public ImportBatch(
        string tenantId,
        ImportType importType,
        string fileName,
        string originalFileName,
        string contentType,
        Guid? createdByUserId)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        ImportType = importType;
        FileName = fileName;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        Status = ImportBatchStatus.Uploaded;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        CreatedByUserId = createdByUserId;
    }

    public void SetValidated(int totalRows, int validRows, int warningRows, int errorRows)
    {
        TotalRows = totalRows;
        ValidRows = validRows;
        WarningRows = warningRows;
        ErrorRows = errorRows;
        Status = ImportBatchStatus.Validated;
    }

    public void SetImported(int importedRows, int skippedRows)
    {
        ImportedRows = importedRows;
        SkippedRows = skippedRows;
        Status = ImportBatchStatus.Imported;
        CompletedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetFailed()
    {
        Status = ImportBatchStatus.Failed;
        CompletedAtUtc = DateTimeOffset.UtcNow;
    }

    internal void AddRow(ImportBatchRow row) => _rows.Add(row);
}

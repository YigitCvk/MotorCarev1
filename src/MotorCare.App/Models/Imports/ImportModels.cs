namespace MotorCare.App.Models.Imports;

public enum ImportType { Customers = 1, Vehicles = 2, ServiceHistory = 3 }
public enum ImportBatchStatus { Uploaded = 1, Parsed = 2, Validated = 3, Imported = 4, Failed = 5, Cancelled = 6 }
public enum ImportRowStatus { Valid = 1, Warning = 2, Error = 3, Imported = 4, Skipped = 5 }

public class ImportBatch
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public ImportType ImportType { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public ImportBatchStatus Status { get; set; }
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int WarningRows { get; set; }
    public int ErrorRows { get; set; }
    public int ImportedRows { get; set; }
    public int SkippedRows { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public List<ImportBatchRow> PreviewRows { get; set; } = [];

    public string ImportTypeDisplay => ImportType switch
    {
        ImportType.Customers => "Müşteriler",
        ImportType.Vehicles => "Araçlar",
        ImportType.ServiceHistory => "Servis Geçmişi",
        _ => ImportType.ToString()
    };

    public string StatusDisplay => Status switch
    {
        ImportBatchStatus.Uploaded => "Yüklendi",
        ImportBatchStatus.Parsed => "Ayrıştırıldı",
        ImportBatchStatus.Validated => "Doğrulandı",
        ImportBatchStatus.Imported => "Aktarıldı",
        ImportBatchStatus.Failed => "Başarısız",
        ImportBatchStatus.Cancelled => "İptal Edildi",
        _ => Status.ToString()
    };

    public bool CanCommit => Status == ImportBatchStatus.Validated && (ValidRows + WarningRows) > 0;
}

public class ImportBatchRow
{
    public Guid Id { get; set; }
    public int RowNumber { get; set; }
    public ImportRowStatus Status { get; set; }
    public string RawJson { get; set; } = string.Empty;
    public string? NormalizedJson { get; set; }
    public string? ErrorMessage { get; set; }
    public string? WarningMessage { get; set; }

    public string StatusDisplay => Status switch
    {
        ImportRowStatus.Valid => "Geçerli",
        ImportRowStatus.Warning => "Uyarı",
        ImportRowStatus.Error => "Hata",
        ImportRowStatus.Imported => "Aktarıldı",
        ImportRowStatus.Skipped => "Atlandı",
        _ => Status.ToString()
    };
}

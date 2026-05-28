namespace MotorCare.Domain.Enums;

public enum ImportBatchStatus
{
    Uploaded = 1,
    Parsed = 2,
    Validated = 3,
    Imported = 4,
    Failed = 5,
    Cancelled = 6
}

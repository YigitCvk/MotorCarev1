namespace MotorCare.App.Models.PublicRecords;

public sealed class PublicRecordAccessResponse
{
    public string TenantId { get; set; } = string.Empty;
    public int RecordType { get; set; }
    public Guid RecordId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? LastAccessedAtUtc { get; set; }
    public int AccessCount { get; set; }
}

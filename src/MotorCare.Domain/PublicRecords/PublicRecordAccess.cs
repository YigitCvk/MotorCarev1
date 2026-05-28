using MotorCare.Domain.Common;

namespace MotorCare.Domain.PublicRecords;

public sealed class PublicRecordAccess : BaseEntity
{
    public string TenantId { get; private set; } = string.Empty;
    public PublicRecordType RecordType { get; private set; }
    public Guid RecordId { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? DisabledAtUtc { get; private set; }
    public DateTimeOffset? LastAccessedAtUtc { get; private set; }
    public int AccessCount { get; private set; }

    private PublicRecordAccess()
    {
    }

    public PublicRecordAccess(string tenantId, PublicRecordType recordType, Guid recordId, string slug, DateTimeOffset createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) throw new DomainException("Tenant ID is required.");
        if (!Enum.IsDefined(recordType)) throw new DomainException("Public record type is invalid.");
        if (recordId == Guid.Empty) throw new DomainException("Record ID is required.");
        if (string.IsNullOrWhiteSpace(slug)) throw new DomainException("Public record slug is required.");

        Id = Guid.NewGuid();
        TenantId = tenantId.Trim();
        RecordType = recordType;
        RecordId = recordId;
        Slug = slug.Trim();
        IsActive = true;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
    }

    public void RegisterAccess(DateTimeOffset accessedAtUtc)
    {
        LastAccessedAtUtc = accessedAtUtc.ToUniversalTime();
        AccessCount++;
    }

    public void Enable()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        DisabledAtUtc = null;
    }

    public void Disable(DateTimeOffset disabledAtUtc)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        DisabledAtUtc = disabledAtUtc.ToUniversalTime();
    }
}

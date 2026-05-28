using MotorCare.Domain.Common;
using MotorCare.Domain.Enums;

namespace MotorCare.Domain.ServiceOrders;

public class ServiceOrderStatusHistory : BaseEntity, ITenantEntity
{
    public string TenantId { get; private set; } = string.Empty;
    public Guid ServiceOrderId { get; private set; }
    public ServiceOrderStatus? FromStatus { get; private set; }
    public ServiceOrderStatus ToStatus { get; private set; }
    public string? Note { get; private set; }
    public Guid? ChangedByUserId { get; private set; }
    public string? ChangedByUserName { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private ServiceOrderStatusHistory()
    {
    }

    public ServiceOrderStatusHistory(
        string tenantId,
        Guid serviceOrderId,
        ServiceOrderStatus? fromStatus,
        ServiceOrderStatus toStatus,
        string? note,
        Guid? changedByUserId,
        string? changedByUserName,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) throw new DomainException("Tenant ID is required.");
        if (serviceOrderId == Guid.Empty) throw new DomainException("Service order ID is required.");
        if (note?.Length > 500) throw new DomainException("Status note cannot exceed 500 charaçters.");

        Id = Guid.NewGuid();
        TenantId = tenantId;
        ServiceOrderId = serviceOrderId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        ChangedByUserId = changedByUserId;
        ChangedByUserName = string.IsNullOrWhiteSpace(changedByUserName) ? null : changedByUserName.Trim();
        CreatedAt = createdAt.ToUniversalTime();
    }
}

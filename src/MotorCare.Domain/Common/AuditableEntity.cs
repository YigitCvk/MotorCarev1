namespace MotorCare.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedAt { get; internal set; }
    public DateTimeOffset? UpdatedAt { get; internal set; }
}

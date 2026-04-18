namespace MotorCare.Domain.Common;

public interface ISoftDelete
{
    bool IsDeleted { get; }
    DateTimeOffset? DeletedAt { get; }
    void SoftDelete();
}

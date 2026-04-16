namespace MotorCare.Domain.Common;

public interface ITenantProvider
{
    string? GetTenantId();
}

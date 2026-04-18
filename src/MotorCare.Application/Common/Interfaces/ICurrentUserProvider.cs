namespace MotorCare.Application.Common.Interfaces;

public interface ICurrentUserProvider
{
    Guid? GetUserId();
    string? GetTenantId();
    string? GetTenantIdentifier();
    string? GetEmail();
    string? GetRole();
}

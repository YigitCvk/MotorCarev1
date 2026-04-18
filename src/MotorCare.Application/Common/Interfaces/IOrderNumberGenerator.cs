namespace MotorCare.Application.Common.Interfaces;

public interface IOrderNumberGenerator
{
    Task<string> GenerateAsync(string tenantId, CancellationToken cancellationToken = default);
}

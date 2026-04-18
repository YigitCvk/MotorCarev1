using MotorCare.Domain.Users;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string tenantId, string normalizedEmail, CancellationToken cancellationToken = default);
    Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void AddRefreshToken(RefreshToken refreshToken);
    void Update(User user);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

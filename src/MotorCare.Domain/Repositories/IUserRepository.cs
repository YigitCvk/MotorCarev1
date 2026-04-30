using MotorCare.Domain.Users;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string tenantId, string normalizedEmail, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);
    Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<UserSecurityToken?> GetActiveSecurityTokenByHashAsync(string tokenHash, UserSecurityTokenPurpose purpose, CancellationToken cancellationToken = default);
    Task<UserSecurityToken?> GetLatestSecurityTokenAsync(Guid userId, UserSecurityTokenPurpose purpose, CancellationToken cancellationToken = default);
    Task<UserSecurityToken?> GetLatestActiveSecurityTokenAsync(Guid userId, UserSecurityTokenPurpose purpose, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void AddRefreshToken(RefreshToken refreshToken);
    void AddSecurityToken(UserSecurityToken securityToken);
    void Update(User user);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

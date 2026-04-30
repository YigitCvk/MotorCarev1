using Microsoft.EntityFrameworkCore;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.Users;
using MotorCare.Domain.Users.Entities;

namespace MotorCare.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId, cancellationToken);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string tenantId, string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == normalizedEmail, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .Where(u => u.Email == normalizedEmail)
            .ToListAsync(cancellationToken);
    }

    public async Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.TokenHash == tokenHash), cancellationToken);
    }

    public async Task<UserSecurityToken?> GetActiveSecurityTokenByHashAsync(string tokenHash, UserSecurityTokenPurpose purpose, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        return await _context.UserSecurityTokens
            .AsTracking()
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash &&
                     x.Purpose == purpose &&
                     x.ExpiresAt > now &&
                     x.RevokedAt == null &&
                     x.ConsumedAt == null,
                cancellationToken);
    }

    public async Task<UserSecurityToken?> GetLatestSecurityTokenAsync(Guid userId, UserSecurityTokenPurpose purpose, CancellationToken cancellationToken = default)
    {
        return await _context.UserSecurityTokens
            .AsTracking()
            .Where(x => x.UserId == userId && x.Purpose == purpose && x.RevokedAt == null && x.ConsumedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public void AddRefreshToken(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
    }

    public void AddSecurityToken(UserSecurityToken securityToken)
    {
        _context.UserSecurityTokens.Add(securityToken);
    }

    public void Update(User user)
    {
        var entry = _context.Entry(user);

        // The aggregate is typically loaded and tracked in the same DbContext scope.
        // Re-attaching it with Update() resets state for the whole graph unnecessarily.
        if (entry.State == EntityState.Detached)
        {
            _context.Users.Update(user);
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}

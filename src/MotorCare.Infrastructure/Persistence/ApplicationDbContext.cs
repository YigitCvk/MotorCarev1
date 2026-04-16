using Microsoft.EntityFrameworkCore;
using MotorCare.Domain.Common;
using MotorCare.Domain.Entities;

namespace MotorCare.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Apply Global Query Filters for Tenancy
        modelBuilder.Entity<Vehicle>().HasQueryFilter(v => v.TenantId == _tenantProvider.GetTenantId());
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Enforce tenancy rules explicitly prior to saving
        var tenantId = _tenantProvider.GetTenantId();
        foreach (var entry in ChangeTracker.Entries<Vehicle>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.TenantId != tenantId)
                {
                    throw new InvalidOperationException("Entity TenantId does not match the current context TenantId.");
                }
            }
        }
        
        return base.SaveChangesAsync(cancellationToken);
    }
}

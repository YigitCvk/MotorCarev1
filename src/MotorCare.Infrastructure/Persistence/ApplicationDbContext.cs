using MotorCare.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MotorCare.Domain.Common;
using MotorCare.Domain.Customers;
using MotorCare.Domain.Vehicles;
using MotorCare.Domain.ServiceOrders;
using MotorCare.Domain.Tenants;

namespace MotorCare.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<ServiceOrder> ServiceOrders => Set<ServiceOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Apply Global Query Filters for Tenancy automatically for all ITenantEntity implementing types
        var tenantId = _tenantProvider.GetTenantId();
        
        modelBuilder.Entity<Customer>().HasQueryFilter(c => c.TenantId == tenantId);
        modelBuilder.Entity<Vehicle>().HasQueryFilter(v => v.TenantId == tenantId);
        modelBuilder.Entity<ServiceOrder>().HasQueryFilter(o => o.TenantId == tenantId);
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantProvider.GetTenantId();
        
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
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

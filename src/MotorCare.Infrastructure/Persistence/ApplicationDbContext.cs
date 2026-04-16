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
    private readonly ITenantProvider? _tenantProvider;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantProvider? tenantProvider = null)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    // EF Core query filters capture this as an expression referencing a DbContext member,
    // so the filter re-evaluates per query using the current tenant context.
    private string CurrentTenantId => _tenantProvider?.GetTenantId() ?? string.Empty;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<ServiceOrder> ServiceOrders => Set<ServiceOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Apply Global Query Filters for Tenancy automatically for all ITenantEntity implementing types
        modelBuilder.Entity<Customer>().HasQueryFilter(c => c.TenantId == CurrentTenantId);
        modelBuilder.Entity<Vehicle>().HasQueryFilter(v => v.TenantId == CurrentTenantId);
        modelBuilder.Entity<ServiceOrder>().HasQueryFilter(o => o.TenantId == CurrentTenantId);
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = CurrentTenantId;
        
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
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                if (!string.IsNullOrEmpty(tenantId) && entry.Entity.TenantId != tenantId)
                {
                    throw new InvalidOperationException("Entity TenantId does not match the current context TenantId.");
                }
            }
        }
        
        return base.SaveChangesAsync(cancellationToken);
    }
}

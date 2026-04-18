using MotorCare.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MotorCare.Domain.Common;
using MotorCare.Domain.Customers;
using MotorCare.Domain.Vehicles;
using MotorCare.Domain.ServiceOrders;
using MotorCare.Domain.Tenants;
using MotorCare.Domain.Users;
using MotorCare.Infrastructure.Persistence.Entities;

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
    public DbSet<ServiceOrderNumberCounter> ServiceOrderNumberCounters => Set<ServiceOrderNumberCounter>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Apply Global Query Filters for Tenancy automatically for all ITenantEntity implementing types
        modelBuilder.Entity<Customer>().HasQueryFilter(c => c.TenantId == CurrentTenantId);
        modelBuilder.Entity<Vehicle>().HasQueryFilter(v => v.TenantId == CurrentTenantId);
        modelBuilder.Entity<ServiceOrder>().HasQueryFilter(o => o.TenantId == CurrentTenantId);
        modelBuilder.Entity<User>().HasQueryFilter(u => u.TenantId == CurrentTenantId);
    }
    
    public override int SaveChanges()
    {
        ApplyAuditInfo();
        EnforceTenantSecurity();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditInfo();
        EnforceTenantSecurity();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        EnforceTenantSecurity();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        EnforceTenantSecurity();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditInfo()
    {
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
    }

    private void EnforceTenantSecurity()
    {
        var tenantId = CurrentTenantId;

        if (string.IsNullOrEmpty(tenantId))
        {
            return;
        }

        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                case EntityState.Modified:
                case EntityState.Deleted:
                    // Guard: entity must belong to the current tenant
                    if (entry.Entity.TenantId != tenantId)
                    {
                        throw new InvalidOperationException(
                            $"Cross-tenant operation detected. Entity TenantId '{entry.Entity.TenantId}' " +
                            $"does not match the current context TenantId '{tenantId}'.");
                    }
                    break;
            }
        }
    }
}

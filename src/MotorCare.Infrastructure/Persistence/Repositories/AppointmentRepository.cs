using Microsoft.EntityFrameworkCore;
using MotorCare.Domain.Appointments;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;

namespace MotorCare.Infrastructure.Persistence.Repositories;

public sealed class AppointmentRepository : IAppointmentRepository
{
    private readonly ApplicationDbContext _context;

    public AppointmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
    }

    public async Task<List<Appointment>> GetFilteredAsync(
        string tenantId,
        DateTimeOffset? startFrom,
        DateTimeOffset? endTo,
        AppointmentStatus? status,
        AppointmentType? type,
        string? searchText,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (startFrom.HasValue)
        {
            query = query.Where(x => x.StartAt >= startFrom.Value);
        }

        if (endTo.HasValue)
        {
            query = query.Where(x => x.StartAt < endTo.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(x => x.Type == type.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim().ToLower();
            query = query.Where(x =>
                x.CustomerName.ToLower().Contains(term) ||
                x.Phone.ToLower().Contains(term) ||
                (x.Plate != null && x.Plate.ToLower().Contains(term)));
        }

        return await query
            .OrderBy(x => x.StartAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Appointment> Items, int TotalCount)> GetFilteredPagedAsync(
        string tenantId,
        DateTimeOffset? startFrom,
        DateTimeOffset? endTo,
        AppointmentStatus? status,
        AppointmentType? type,
        string? searchText,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (startFrom.HasValue)
            query = query.Where(x => x.StartAt >= startFrom.Value);

        if (endTo.HasValue)
            query = query.Where(x => x.StartAt < endTo.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (type.HasValue)
            query = query.Where(x => x.Type == type.Value);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim().ToLower();
            query = query.Where(x =>
                x.CustomerName.ToLower().Contains(term) ||
                x.Phone.ToLower().Contains(term) ||
                (x.Plate != null && x.Plate.ToLower().Contains(term)));
        }

        query = query.OrderBy(x => x.StartAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<Appointment>> GetByCustomerIdAsync(string tenantId, Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CustomerId == customerId)
            .OrderByDescending(x => x.StartAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        await _context.Appointments.AddAsync(appointment, cancellationToken);
    }

    public void Update(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}

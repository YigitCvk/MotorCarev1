using MotorCare.Domain.Appointments;
using MotorCare.Domain.Enums;

namespace MotorCare.Domain.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default);
    Task<List<Appointment>> GetFilteredAsync(
        string tenantId,
        DateTimeOffset? startFrom,
        DateTimeOffset? endTo,
        AppointmentStatus? status,
        AppointmentType? type,
        string? searchText,
        CancellationToken cancellationToken = default);
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
    void Update(Appointment appointment);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

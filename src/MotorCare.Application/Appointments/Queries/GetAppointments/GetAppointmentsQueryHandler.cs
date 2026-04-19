using MediatR;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.Common.Models;
using MotorCare.Domain.Repositories;

namespace MotorCare.Application.Appointments.Queries.GetAppointments;

public sealed class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, PagedResult<AppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ITenantProvider _tenantProvider;

    public GetAppointmentsQueryHandler(IAppointmentRepository appointmentRepository, ITenantProvider tenantProvider)
    {
        _appointmentRepository = appointmentRepository;
        _tenantProvider = tenantProvider;
    }

    public async Task<PagedResult<AppointmentDto>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        var pagination = PaginationRequest.Of(request.PageNumber, request.PageSize);

        var (appointments, totalCount) = await _appointmentRepository.GetFilteredPagedAsync(
            tenantId,
            request.StartFrom,
            request.EndTo,
            request.Status,
            request.Type,
            request.SearchText,
            pagination.SafePageNumber,
            pagination.SafePageSize,
            cancellationToken);

        var items = appointments
            .Select(a => new AppointmentDto(
                a.Id,
                a.CustomerId,
                a.VehicleId,
                a.CustomerName,
                a.Phone,
                a.Plate,
                a.Type,
                AppointmentTextMapper.ToText(a.Type),
                a.Status,
                AppointmentTextMapper.ToText(a.Status),
                a.StartAt,
                a.EndAt,
                a.Note,
                a.Complaint,
                a.ServiceOrderId))
            .ToList();

        return PagedResult<AppointmentDto>.Create(items, pagination.SafePageNumber, pagination.SafePageSize, totalCount);
    }
}

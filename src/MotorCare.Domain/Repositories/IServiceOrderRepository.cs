using MotorCare.Domain.Enums;
using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Domain.Repositories;

public interface IServiceOrderRepository
{
    Task<ServiceOrder?> GetByIdAsync(Guid id, string tenantId, CancellationToken cancellationToken = default);
    Task<ServiceOrder?> GetByOrderNoAsync(string tenantId, string orderNo, CancellationToken cancellationToken = default);
    Task<List<ServiceOrder>> GetAllAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<ServiceOrderDailySummary> GetDailySummaryAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<List<ServiceOrder>> GetFilteredAsync(
        string tenantId,
        Guid? customerId,
        ServiceOrderStatus? status,
        string? searchText,
        DateTimeOffset? openedFrom,
        DateTimeOffset? openedTo,
        CancellationToken cancellationToken = default);
    Task<(List<ServiceOrder> Items, int TotalCount)> GetFilteredPagedAsync(
        string tenantId,
        Guid? customerId,
        ServiceOrderStatus? status,
        string? searchText,
        DateTimeOffset? openedFrom,
        DateTimeOffset? openedTo,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<List<ServiceOrderDashboardSnapshot>> GetRecentDashboardOrdersAsync(
        string tenantId,
        int take,
        CancellationToken cancellationToken = default);
    Task<(List<ServiceOrder> Items, int TotalCount)> GetByVehicleIdAsync(Guid vehicleId, string tenantId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceOrderStatusHistory>> GetStatusHistoryAsync(Guid serviceOrderId, string tenantId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, string tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceOrderAttachment>> GetAttachmentsAsync(Guid serviceOrderId, string tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceOrderAttachment>> GetAttachmentsIncludingDeletedAsync(Guid serviceOrderId, string tenantId, CancellationToken cancellationToken = default);
    Task<ServiceOrderAttachment?> GetAttachmentAsync(Guid serviceOrderId, Guid attachmentId, string tenantId, CancellationToken cancellationToken = default);
    Task<int> GetTodayOrderCountAsync(string tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(ServiceOrder order, CancellationToken cancellationToken = default);
    Task AddStatusHistoryAsync(ServiceOrderStatusHistory history, CancellationToken cancellationToken = default);
    Task AddAttachmentAsync(ServiceOrderAttachment attachment, CancellationToken cancellationToken = default);
    void Update(ServiceOrder order);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed record ServiceOrderDashboardSnapshot(
    Guid Id,
    string OrderNo,
    Guid CustomerId,
    Guid VehicleId,
    ServiceOrderStatus Status,
    DateTimeOffset OpenedAt,
    decimal GrandTotal);

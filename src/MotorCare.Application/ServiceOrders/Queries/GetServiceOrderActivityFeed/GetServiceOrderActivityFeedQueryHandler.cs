using MediatR;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ServiceOrders;
using MotorCare.Domain.ServiceOrders.Entities;

namespace MotorCare.Application.ServiceOrders.Queries.GetServiceOrderActivityFeed;

public sealed class GetServiceOrderActivityFeedQueryHandler
    : IRequestHandler<GetServiceOrderActivityFeedQuery, IReadOnlyList<ServiceOrderActivityFeedItem>>
{
    private const string Currency = "TRY";
    private const string SystemActor = "Sistem";

    private readonly IServiceOrderRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetServiceOrderActivityFeedQueryHandler> _logger;

    public GetServiceOrderActivityFeedQueryHandler(
        IServiceOrderRepository repository,
        IUserRepository userRepository,
        ITenantProvider tenantProvider,
        ICurrentUserProvider currentUserProvider,
        ILogger<GetServiceOrderActivityFeedQueryHandler> logger)
    {
        _repository = repository;
        _userRepository = userRepository;
        _tenantProvider = tenantProvider;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ServiceOrderActivityFeedItem>> Handle(
        GetServiceOrderActivityFeedQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId()
            ?? throw new UnauthorizedAccessException("Tenant ID is required.");

        try
        {
            var order = await _repository.GetByIdAsync(request.ServiceOrderId, tenantId, cancellationToken)
                ?? throw new NotFoundException(nameof(ServiceOrder), request.ServiceOrderId);

            var statusHistory = await _repository.GetStatusHistoryAsync(order.Id, tenantId, cancellationToken);
            var attachments = await _repository.GetAttachmentsIncludingDeletedAsync(order.Id, tenantId, cancellationToken);
            var deletedAttachmentActors = await GetDeletedAttachmentActorsAsync(attachments, tenantId, cancellationToken);
            var items = new List<ServiceOrderActivityFeedItem>();
            var createdHistory = statusHistory.FirstOrDefault(x => x.FromStatus is null);
            var orderCreatedAt = NormalizeCreatedAt(order.CreatedAt, order.OpenedAt);

            items.Add(new ServiceOrderActivityFeedItem(
                order.Id,
                "ServiceOrderCreated",
                "Servis Emri Oluşturuldu",
                "Servis emri oluşturuldu",
                $"{order.OrderNo} numaralı servis emri açıldı.",
                ActorOrSystem(createdHistory?.ChangedByUserName),
                orderCreatedAt,
                null,
                Currency,
                order.Id,
                nameof(ServiceOrder),
                "history",
                "info"));

            foreach (var history in statusHistory.Where(x => x.FromStatus.HasValue))
            {
                var description = $"{ToStatusText(history.FromStatus!.Value)} -> {ToStatusText(history.ToStatus)}";
                if (!string.IsNullOrWhiteSpace(history.Note))
                {
                    description = $"{description}. Not: {history.Note.Trim()}";
                }

                items.Add(new ServiceOrderActivityFeedItem(
                    history.Id,
                    "StatusChanged",
                    "Durum Değişti",
                    "Durum değişti",
                    description,
                    ActorOrSystem(history.ChangedByUserName),
                    NormalizeCreatedAt(history.CreatedAt, orderCreatedAt),
                    null,
                    Currency,
                    history.Id,
                    nameof(ServiceOrderStatusHistory),
                    "history",
                    ToStatusSeverity(history.ToStatus)));
            }

            AddOperationActivities(order, items, orderCreatedAt);
            AddPartActivities(order, items, orderCreatedAt);
            AddConsumableActivities(order, items, orderCreatedAt);
            AddPaymentActivities(order, items, orderCreatedAt);
            AddAttachmentActivities(attachments, deletedAttachmentActors, items, orderCreatedAt);

            var result = items
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Title, StringComparer.Ordinal)
                .ToList();

            _logger.LogDebug(
                EventIdStore.ServiceOrder.ServiceOrderActivityFeedFetched,
                "Service order activity feed fetched. ServiceOrderId={ServiceOrderId} TenantId={TenantId} ActivityCount={ActivityCount}",
                order.Id,
                tenantId,
                result.Count);

            return result;
        }
        catch (Exception ex) when (ex is not NotFoundException and not OperationCanceledException)
        {
            _logger.LogError(
                EventIdStore.ServiceOrder.ServiceOrderActivityFeedFailed,
                ex,
                "Service order activity feed failed. ServiceOrderId={ServiceOrderId} TenantId={TenantId} UserId={UserId} Error={Error}",
                request.ServiceOrderId,
                tenantId,
                _currentUserProvider.GetUserId(),
                ex.Message);
            throw;
        }
    }

    private async Task<IReadOnlyDictionary<Guid, string>> GetDeletedAttachmentActorsAsync(
        IReadOnlyList<ServiceOrderAttachment> attachments,
        string tenantId,
        CancellationToken cancellationToken)
    {
        var actorIds = attachments
            .Where(x => x.IsDeleted && x.DeletedByUserId.HasValue)
            .Select(x => x.DeletedByUserId!.Value)
            .Distinct()
            .ToList();

        if (actorIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var actors = new Dictionary<Guid, string>();
        foreach (var actorId in actorIds)
        {
            var user = await _userRepository.GetByIdAsync(actorId, tenantId, cancellationToken);
            if (user is not null)
            {
                actors[actorId] = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName;
            }
        }

        return actors;
    }

    private static void AddOperationActivities(
        ServiceOrder order,
        ICollection<ServiceOrderActivityFeedItem> items,
        DateTimeOffset fallbackCreatedAt)
    {
        foreach (var operation in order.Operations)
        {
            items.Add(new ServiceOrderActivityFeedItem(
                operation.Id,
                "LaborAdded",
                "İşçilik Eklendi",
                "İşçilik eklendi",
                string.IsNullOrWhiteSpace(operation.Description)
                    ? "İşçilik kalemi eklendi."
                    : operation.Description,
                SystemActor,
                NormalizeCreatedAt(operation.CreatedAt, fallbackCreatedAt),
                operation.Price,
                Currency,
                operation.Id,
                nameof(ServiceOperationItem),
                "wrench",
                "info"));
        }
    }

    private static void AddPartActivities(
        ServiceOrder order,
        ICollection<ServiceOrderActivityFeedItem> items,
        DateTimeOffset fallbackCreatedAt)
    {
        foreach (var part in order.Parts)
        {
            items.Add(new ServiceOrderActivityFeedItem(
                part.Id,
                "PartAdded",
                "Parça Eklendi",
                "Parça eklendi",
                $"{part.PartName} - {part.Quantity} adet",
                SystemActor,
                NormalizeCreatedAt(part.CreatedAt, fallbackCreatedAt),
                part.TotalPrice,
                Currency,
                part.Id,
                nameof(ServicePartItem),
                "package",
                "info"));
        }
    }

    private static void AddConsumableActivities(
        ServiceOrder order,
        ICollection<ServiceOrderActivityFeedItem> items,
        DateTimeOffset fallbackCreatedAt)
    {
        foreach (var consumable in order.Consumables)
        {
            var brand = string.IsNullOrWhiteSpace(consumable.Brand) ? null : consumable.Brand;
            var description = brand is null
                ? $"{consumable.Category} - {consumable.ProductName}"
                : $"{consumable.Category} - {brand} {consumable.ProductName}";

            items.Add(new ServiceOrderActivityFeedItem(
                consumable.Id,
                "ConsumableAdded",
                "Sarf Ürün Eklendi",
                "Sarf ürün eklendi",
                description,
                SystemActor,
                NormalizeCreatedAt(consumable.CreatedAt, fallbackCreatedAt),
                null,
                Currency,
                consumable.Id,
                nameof(ServiceConsumableItem),
                "package",
                "info"));
        }
    }

    private static void AddPaymentActivities(
        ServiceOrder order,
        ICollection<ServiceOrderActivityFeedItem> items,
        DateTimeOffset fallbackCreatedAt)
    {
        foreach (var payment in order.Payments)
        {
            items.Add(new ServiceOrderActivityFeedItem(
                payment.Id,
                "PaymentAdded",
                "Tahsilat Eklendi",
                "Tahsilat eklendi",
                ToPaymentMethodText(payment.Method),
                SystemActor,
                NormalizeCreatedAt(payment.CreatedAt, payment.PaymentDate == default ? fallbackCreatedAt : payment.PaymentDate),
                payment.Amount,
                Currency,
                payment.Id,
                nameof(ServicePayment),
                "credit-card",
                "success"));
        }
    }

    private static void AddAttachmentActivities(
        IReadOnlyList<ServiceOrderAttachment> attachments,
        IReadOnlyDictionary<Guid, string> deletedAttachmentActors,
        ICollection<ServiceOrderActivityFeedItem> items,
        DateTimeOffset fallbackCreatedAt)
    {
        foreach (var attachment in attachments.Where(x => !x.IsDeleted))
        {
            items.Add(new ServiceOrderActivityFeedItem(
                attachment.Id,
                "AttachmentUploaded",
                "Dosya Yüklendi",
                "Dosya yüklendi",
                $"{ServiceOrderAttachmentMapper.GetAttachmentTypeText(attachment.AttachmentType)} - {attachment.OriginalFileName}",
                ActorOrSystem(attachment.UploadedByUserName),
                NormalizeCreatedAt(attachment.CreatedAt, fallbackCreatedAt),
                null,
                Currency,
                attachment.Id,
                nameof(ServiceOrderAttachment),
                "file",
                "info"));
        }

        foreach (var attachment in attachments.Where(x => x.IsDeleted))
        {
            var actorName = attachment.DeletedByUserId.HasValue &&
                            deletedAttachmentActors.TryGetValue(attachment.DeletedByUserId.Value, out var deletedBy)
                ? deletedBy
                : SystemActor;

            items.Add(new ServiceOrderActivityFeedItem(
                attachment.Id,
                "AttachmentDeleted",
                "Dosya Silindi",
                "Dosya silindi",
                $"{ServiceOrderAttachmentMapper.GetAttachmentTypeText(attachment.AttachmentType)} - {attachment.OriginalFileName}",
                actorName,
                NormalizeCreatedAt(attachment.DeletedAt ?? attachment.CreatedAt, fallbackCreatedAt),
                null,
                Currency,
                attachment.Id,
                nameof(ServiceOrderAttachment),
                "file",
                "warning"));
        }
    }

    private static DateTimeOffset NormalizeCreatedAt(DateTimeOffset value, DateTimeOffset fallback)
        => value == default ? fallback : value;

    private static string ActorOrSystem(string? actorName)
        => string.IsNullOrWhiteSpace(actorName) ? SystemActor : actorName.Trim();

    private static string ToStatusText(ServiceOrderStatus status) => status switch
    {
        ServiceOrderStatus.Open => "Açık",
        ServiceOrderStatus.InProgress => "İşlemde",
        ServiceOrderStatus.WaitingForParts => "Parça Bekliyor",
        ServiceOrderStatus.Completed => "Teslime Hazır",
        ServiceOrderStatus.Delivered => "Teslim Edildi",
        ServiceOrderStatus.Cancelled => "İptal Edildi",
        _ => status.ToString()
    };

    private static string ToStatusSeverity(ServiceOrderStatus status) => status switch
    {
        ServiceOrderStatus.Completed or ServiceOrderStatus.Delivered => "success",
        ServiceOrderStatus.WaitingForParts => "warning",
        ServiceOrderStatus.Cancelled => "danger",
        _ => "info"
    };

    private static string ToPaymentMethodText(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash => "Nakit tahsilat",
        PaymentMethod.CreditCard => "Kredi kartı tahsilatı",
        PaymentMethod.BankTransfer => "Havale / EFT tahsilatı",
        _ => method.ToString()
    };
}

using MediatR;

namespace MotorCare.Application.ServiceOrders.Queries.GetServiceOrderActivityFeed;

public sealed record GetServiceOrderActivityFeedQuery(Guid ServiceOrderId)
    : IRequest<IReadOnlyList<ServiceOrderActivityFeedItem>>;

public sealed record ServiceOrderActivityFeedItem(
    Guid Id,
    string ActivityType,
    string ActivityTypeText,
    string Title,
    string Description,
    string ActorName,
    DateTimeOffset CreatedAt,
    decimal? Amount,
    string Currency,
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    string IconName,
    string Severity);

namespace MotorCare.App.Models.ServiceOrders;

public sealed class ServiceOrderActivityFeedItem
{
    public Guid Id { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string ActivityTypeText { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ActorName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public decimal? Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public string IconName { get; set; } = string.Empty;
    public string Severity { get; set; } = "info";
}

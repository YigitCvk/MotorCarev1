using MotorCare.Domain.Enums;

namespace MotorCare.Application.Services;

public sealed record ServiceCatalogItemDto(
    Guid Id,
    string Name,
    ServiceCategory Category,
    string CategoryText,
    string? Description,
    int DefaultDurationMinutes,
    decimal DefaultPrice,
    decimal Price,
    string Currency,
    bool IsActive);

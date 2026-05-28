using MediatR;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.Services.Commands.UpdateServiceCatalogItem;

public sealed record UpdateServiceCatalogItemCommand(
    Guid Id,
    string Name,
    ServiceCategory Category,
    string? Description,
    int DefaultDurationMinutes,
    decimal Price,
    string Currency,
    bool IsActive) : IRequest;

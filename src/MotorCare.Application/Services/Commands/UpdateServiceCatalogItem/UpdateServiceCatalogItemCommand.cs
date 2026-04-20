using MediatR;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.Services.Commands.UpdateServiceCatalogItem;

public sealed record UpdateServiceCatalogItemCommand(
    Guid Id,
    string Name,
    ServiceCategory Category,
    string? Description,
    int DefaultDurationMinutes,
    decimal DefaultPrice,
    bool IsActive) : IRequest;

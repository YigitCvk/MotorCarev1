using MediatR;
using MotorCare.Domain.Enums;

namespace MotorCare.Application.Services.Commands.CreateServiceCatalogItem;

public sealed record CreateServiceCatalogItemCommand(
    string Name,
    ServiceCategory Category,
    string? Description,
    int DefaultDurationMinutes,
    decimal Price,
    string Currency = "TRY",
    bool IsActive = true) : IRequest<Guid>;

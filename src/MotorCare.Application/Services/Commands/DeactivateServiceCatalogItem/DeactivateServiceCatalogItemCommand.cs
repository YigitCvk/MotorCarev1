using MediatR;

namespace MotorCare.Application.Services.Commands.DeactivateServiceCatalogItem;

public sealed record DeactivateServiceCatalogItemCommand(Guid Id) : IRequest;

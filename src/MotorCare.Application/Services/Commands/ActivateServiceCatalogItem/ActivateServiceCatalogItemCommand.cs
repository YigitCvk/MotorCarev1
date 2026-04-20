using MediatR;

namespace MotorCare.Application.Services.Commands.ActivateServiceCatalogItem;

public sealed record ActivateServiceCatalogItemCommand(Guid Id) : IRequest;

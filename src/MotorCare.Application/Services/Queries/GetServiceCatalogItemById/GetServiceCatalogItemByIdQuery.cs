using MediatR;

namespace MotorCare.Application.Services.Queries.GetServiceCatalogItemById;

public sealed record GetServiceCatalogItemByIdQuery(Guid Id) : IRequest<ServiceCatalogItemDto?>;

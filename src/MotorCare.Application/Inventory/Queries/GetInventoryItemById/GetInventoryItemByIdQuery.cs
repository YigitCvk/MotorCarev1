using MediatR;

namespace MotorCare.Application.Inventory.Queries.GetInventoryItemById;

public sealed record GetInventoryItemByIdQuery(Guid Id) : IRequest<InventoryItemDto?>;

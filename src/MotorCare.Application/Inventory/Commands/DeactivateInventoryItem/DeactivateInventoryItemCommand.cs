using MediatR;

namespace MotorCare.Application.Inventory.Commands.DeactivateInventoryItem;

public sealed record DeactivateInventoryItemCommand(Guid Id) : IRequest;

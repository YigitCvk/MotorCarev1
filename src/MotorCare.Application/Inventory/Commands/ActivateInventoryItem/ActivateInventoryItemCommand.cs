using MediatR;

namespace MotorCare.Application.Inventory.Commands.ActivateInventoryItem;

public sealed record ActivateInventoryItemCommand(Guid Id) : IRequest;

using MediatR;

namespace MotorCare.Application.Inventory.Commands.AdjustInventoryStock;

public sealed record AdjustInventoryStockCommand(Guid Id, decimal QuantityDelta, string Reason) : IRequest;

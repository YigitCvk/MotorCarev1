using MediatR;
using MotorCare.Application.Common.Models;

namespace MotorCare.Application.Inventory.Queries.GetInventoryItems;

public sealed record GetInventoryItemsQuery(
    string? SearchText,
    string? Category,
    bool? IsActive,
    bool LowStockOnly,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<InventoryItemDto>>;

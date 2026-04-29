namespace MotorCare.Application.ServiceOrders;

public sealed record ConsumableCatalogItemDto(
    string Category,
    string? SubCategory,
    string Brand,
    string ProductName,
    string? Specification,
    string? Notes);

public sealed record ConsumableCatalogItemInput(
    string Category,
    string? SubCategory,
    string Brand,
    string ProductName,
    string? Specification,
    string? Notes);

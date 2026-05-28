using Microsoft.Extensions.Logging;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.ServiceOrders.Commands.RemovePartFromOrder;
using MotorCare.Domain.Inventory;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Application.UnitTests.ServiceOrders;

public class RemovePartFromOrderCommandHandlerTests
{
    private readonly IServiceOrderRepository _orderRepo = Substitute.For<IServiceOrderRepository>();
    private readonly IInventoryRepository _inventoryRepo = Substitute.For<IInventoryRepository>();
    private readonly ITenantProvider _tenantProvider = Substitute.For<ITenantProvider>();
    private readonly ILogger<RemovePartFromOrderCommandHandler> _logger =
        Substitute.For<ILogger<RemovePartFromOrderCommandHandler>>();

    private readonly RemovePartFromOrderCommandHandler _handler;

    private const string TenantId = "test-tenant";
    private static readonly Guid OrderId = Guid.NewGuid();
    private static readonly Guid InventoryItemId = Guid.NewGuid();

    public RemovePartFromOrderCommandHandlerTests()
    {
        _tenantProvider.GetTenantId().Returns(TenantId);
        _handler = new RemovePartFromOrderCommandHandler(_orderRepo, _inventoryRepo, _tenantProvider, _logger);
    }

    private static ServiceOrder MakeOrder()
    {
        return new ServiceOrder(TenantId, "SO-001", Guid.NewGuid(), Guid.NewGuid(), 1000, null);
    }

    private static InventoryItem MakeInventoryItem(decimal stock = 10m)
    {
        return new InventoryItem(TenantId, "Test Part", null, null, null, null, "adet", 50m, stock, 0);
    }

    [Fact]
    public async Task Handle_RestoresInventory_WhenInventoryItemIdIsSet()
    {
        var order = MakeOrder();
        order.AddPart("Oil Filter", "F-001", 50m, 2, InventoryItemId);
        var part = order.Parts.Single();

        var inventoryItem = MakeInventoryItem(stock: 5m);

        _orderRepo.GetByIdAsync(OrderId, TenantId, default).Returns(order);
        _inventoryRepo.GetByIdAsync(InventoryItemId, TenantId, default).Returns(inventoryItem);

        await _handler.Handle(new RemovePartFromOrderCommand(OrderId, part.Id), default);

        inventoryItem.StockQuantity.Should().Be(7m, "restoring 2 units to 5 initial = 7");
        _inventoryRepo.Received(1).Update(inventoryItem);
    }

    [Fact]
    public async Task Handle_DoesNotRestoreInventory_WhenInventoryItemIdIsNull()
    {
        var order = MakeOrder();
        order.AddPart("Custom Part", null, 30m, 1);
        var part = order.Parts.Single();

        _orderRepo.GetByIdAsync(OrderId, TenantId, default).Returns(order);

        await _handler.Handle(new RemovePartFromOrderCommand(OrderId, part.Id), default);

        await _inventoryRepo.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        _inventoryRepo.DidNotReceive().Update(Arg.Any<InventoryItem>());
    }

    [Fact]
    public async Task Handle_SkipsInventoryRestore_WhenInventoryItemNotFound()
    {
        var order = MakeOrder();
        order.AddPart("Deleted Part", null, 20m, 3, InventoryItemId);
        var part = order.Parts.Single();

        _orderRepo.GetByIdAsync(OrderId, TenantId, default).Returns(order);
        _inventoryRepo.GetByIdAsync(InventoryItemId, TenantId, default).Returns((InventoryItem?)null);

        // Should not throw even if inventory item no longer exists
        var act = async () => await _handler.Handle(new RemovePartFromOrderCommand(OrderId, part.Id), default);
        await act.Should().NotThrowAsync();

        _inventoryRepo.DidNotReceive().Update(Arg.Any<InventoryItem>());
    }

    [Fact]
    public async Task Handle_RemovesPart_FromOrderRegardlessOfInventory()
    {
        var order = MakeOrder();
        order.AddPart("Filter", "F-002", 40m, 1, InventoryItemId);
        var part = order.Parts.Single();
        var inventoryItem = MakeInventoryItem();

        _orderRepo.GetByIdAsync(OrderId, TenantId, default).Returns(order);
        _inventoryRepo.GetByIdAsync(InventoryItemId, TenantId, default).Returns(inventoryItem);

        await _handler.Handle(new RemovePartFromOrderCommand(OrderId, part.Id), default);

        order.Parts.Should().BeEmpty("part must be removed from the order");
        _orderRepo.Received(1).Update(order);
        await _orderRepo.Received(1).SaveChangesAsync(default);
    }
}

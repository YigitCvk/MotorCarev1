using Microsoft.Extensions.Logging;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.ServiceOrders.Commands.RemoveConsumableFromOrder;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Application.UnitTests.ServiceOrders;

public class RemoveConsumableFromOrderCommandHandlerTests
{
    private readonly IServiceOrderRepository _orderRepo = Substitute.For<IServiceOrderRepository>();
    private readonly ITenantProvider _tenantProvider = Substitute.For<ITenantProvider>();
    private readonly ILogger<RemoveConsumableFromOrderCommandHandler> _logger =
        Substitute.For<ILogger<RemoveConsumableFromOrderCommandHandler>>();

    private readonly RemoveConsumableFromOrderCommandHandler _handler;

    private const string TenantId = "test-tenant";
    private static readonly Guid OrderId = Guid.NewGuid();

    public RemoveConsumableFromOrderCommandHandlerTests()
    {
        _tenantProvider.GetTenantId().Returns(TenantId);
        _handler = new RemoveConsumableFromOrderCommandHandler(_orderRepo, _tenantProvider, _logger);
    }

    private static ServiceOrder MakeOrder()
    {
        return new ServiceOrder(TenantId, "SO-001", Guid.NewGuid(), Guid.NewGuid(), 1000, null);
    }

    [Fact]
    public async Task Handle_RemovesConsumable_WhenConsumableExists()
    {
        var order = MakeOrder();
        order.AddConsumable("Motor Yagi", "10W-40", 75m, 2, brand: "Motul");
        var consumable = order.Consumables.Single();

        _orderRepo.GetByIdAsync(OrderId, TenantId, default).Returns(order);

        await _handler.Handle(new RemoveConsumableFromOrderCommand(OrderId, consumable.Id), default);

        order.Consumables.Should().BeEmpty("consumable must be removed from the order");
        order.ConsumablesTotal.Should().Be(0m);
        _orderRepo.Received(1).Update(order);
        await _orderRepo.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenOrderNotFound()
    {
        _orderRepo.GetByIdAsync(OrderId, TenantId, default).Returns((ServiceOrder?)null);

        var act = async () => await _handler.Handle(new RemoveConsumableFromOrderCommand(OrderId, Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

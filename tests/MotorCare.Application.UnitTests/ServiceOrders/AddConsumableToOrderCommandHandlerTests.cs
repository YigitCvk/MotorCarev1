using Microsoft.Extensions.Logging;
using MotorCare.Application.Common.Exceptions;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.ServiceOrders.Commands.AddConsumableToOrder;
using MotorCare.Domain.Repositories;
using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Application.UnitTests.ServiceOrders;

public class AddConsumableToOrderCommandHandlerTests
{
    private readonly IServiceOrderRepository _orderRepo = Substitute.For<IServiceOrderRepository>();
    private readonly ITenantProvider _tenantProvider = Substitute.For<ITenantProvider>();
    private readonly ILogger<AddConsumableToOrderCommandHandler> _logger =
        Substitute.For<ILogger<AddConsumableToOrderCommandHandler>>();

    private readonly AddConsumableToOrderCommandHandler _handler;

    private const string TenantId = "test-tenant";
    private static readonly Guid OrderId = Guid.NewGuid();

    public AddConsumableToOrderCommandHandlerTests()
    {
        _tenantProvider.GetTenantId().Returns(TenantId);
        _handler = new AddConsumableToOrderCommandHandler(_orderRepo, _tenantProvider, _logger);
    }

    private static ServiceOrder MakeOrder()
    {
        return new ServiceOrder(TenantId, "SO-001", Guid.NewGuid(), Guid.NewGuid(), 1000, null);
    }

    private static AddConsumableToOrderCommand MakeCommand(Guid? id = null)
    {
        return new AddConsumableToOrderCommand(
            id ?? OrderId,
            "Motor Yagi",
            "10W-40",
            75m,
            2,
            Brand: "Motul");
    }

    [Fact]
    public async Task Handle_AddsConsumable_WhenOrderExists()
    {
        var order = MakeOrder();
        _orderRepo.GetByIdAsync(OrderId, TenantId, default).Returns(order);

        await _handler.Handle(MakeCommand(), default);

        order.Consumables.Should().HaveCount(1);
        order.ConsumablesTotal.Should().Be(150m);
        _orderRepo.Received(1).Update(order);
        await _orderRepo.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenOrderNotFound()
    {
        _orderRepo.GetByIdAsync(OrderId, TenantId, default).Returns((ServiceOrder?)null);

        var act = async () => await _handler.Handle(MakeCommand(), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ThrowsUnauthorized_WhenTenantMissing()
    {
        _tenantProvider.GetTenantId().Returns((string?)null);

        var act = async () => await _handler.Handle(MakeCommand(), default);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}

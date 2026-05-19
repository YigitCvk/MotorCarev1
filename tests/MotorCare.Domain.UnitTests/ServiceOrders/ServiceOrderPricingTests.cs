using MotorCare.Domain.Common;
using MotorCare.Domain.ServiceOrders;

namespace MotorCare.Domain.UnitTests.ServiceOrders;

public class ServiceOrderPricingTests
{
    [Fact]
    public void AddOperation_ShouldCalculateLaborAndGrandTotals_WithQuantityUnitPriceAndDiscount()
    {
        var order = CreateOrder();

        order.AddOperation("Bakim isciligi", 2m, 300m, 50m);

        var operation = Assert.Single(order.Operations);
        Assert.Equal(2m, operation.Quantity);
        Assert.Equal(300m, operation.UnitPrice);
        Assert.Equal(50m, operation.Discount);
        Assert.Equal(550m, operation.LineTotal);
        Assert.Equal(550m, order.LaborTotal);
        Assert.Equal(550m, order.GrandTotal);
    }

    [Fact]
    public void AddPart_ShouldCalculatePartsAndGrandTotals_WithDiscount()
    {
        var order = CreateOrder();

        order.AddPart("Yag filtresi", "HF204", 100m, 2, discount: 25m);

        var part = Assert.Single(order.Parts);
        Assert.Equal(175m, part.LineTotal);
        Assert.Equal(175m, order.PartsTotal);
        Assert.Equal(175m, order.GrandTotal);
    }

    [Fact]
    public void AddConsumable_ShouldCalculateConsumablesAndGrandTotals()
    {
        var order = CreateOrder();

        order.AddConsumable("Motor yagi", "10W-40", 75m, 2, brand: "Motul");

        var consumable = Assert.Single(order.Consumables);
        Assert.Equal(150m, consumable.LineTotal);
        Assert.Equal(150m, order.ConsumablesTotal);
        Assert.Equal(150m, order.GrandTotal);
    }

    [Fact]
    public void AddPart_ShouldRejectDiscountGreaterThanLineGross()
    {
        var order = CreateOrder();

        var ex = Assert.Throws<DomainException>(
            () => order.AddPart("Balata", null, 100m, 1, discount: 101m));

        Assert.Contains("Discount cannot exceed line total", ex.Message);
    }

    [Fact]
    public void AddConsumable_WithValidPricing_CalculatesConsumablesTotal()
    {
        var order = CreateOrder();

        order.AddConsumable("Yag", "10W-40", 80m, 3);

        Assert.Equal(240m, order.ConsumablesTotal);
    }

    [Fact]
    public void AddConsumable_WithZeroUnitPrice_IsAllowed()
    {
        var order = CreateOrder();

        var ex = Record.Exception(() => order.AddConsumable("Yag", "10W-40", 0m, 1));

        Assert.Null(ex);
        Assert.Single(order.Consumables);
        Assert.Equal(0m, order.ConsumablesTotal);
    }

    [Fact]
    public void AddConsumable_WithNegativeUnitPrice_ThrowsDomainException()
    {
        var order = CreateOrder();

        Assert.Throws<DomainException>(() => order.AddConsumable("Yag", "10W-40", -1m, 1));
    }

    [Fact]
    public void AddConsumable_WithZeroQuantity_ThrowsDomainException()
    {
        var order = CreateOrder();

        Assert.Throws<DomainException>(() => order.AddConsumable("Yag", "10W-40", 50m, 0));
    }

    [Fact]
    public void AddConsumable_IncrementsConsumablesTotal_AndGrandTotal()
    {
        var order = CreateOrder();

        order.AddConsumable("Yag", "10W-40", 100m, 2);
        order.AddConsumable("Antifriz", "OAT", 50m, 1);

        Assert.Equal(250m, order.ConsumablesTotal);
        Assert.Equal(250m, order.GrandTotal);
    }

    [Fact]
    public void RemoveConsumable_DecrementsConsumablesTotal_AndGrandTotal()
    {
        var order = CreateOrder();
        order.AddConsumable("Yag", "10W-40", 100m, 2);
        order.AddConsumable("Antifriz", "OAT", 50m, 1);
        var toRemove = order.Consumables.First();

        order.RemoveConsumable(toRemove.Id);

        Assert.Single(order.Consumables);
        Assert.DoesNotContain(order.Consumables, c => c.Id == toRemove.Id);
        Assert.Equal(order.Consumables.Sum(c => c.LineTotal), order.ConsumablesTotal);
        Assert.Equal(order.ConsumablesTotal, order.GrandTotal);
    }

    [Fact]
    public void RemoveConsumable_WhenNotFound_ThrowsDomainException()
    {
        var order = CreateOrder();

        Assert.Throws<DomainException>(() => order.RemoveConsumable(Guid.NewGuid()));
    }

    [Fact]
    public void SetDiscount_CannotExceedLaborPlusPartsPlus_ConsumablesTotal()
    {
        var order = CreateOrder();
        order.AddOperation("Iscilik", 1m, 200m);
        order.AddPart("Filtre", null, 100m, 1);
        order.AddConsumable("Yag", "10W-40", 50m, 1);

        // LaborTotal=200, PartsTotal=100, ConsumablesTotal=50 => sum=350
        var ex = Assert.Throws<DomainException>(() => order.SetDiscount(351m));

        Assert.Contains("Discount cannot exceed", ex.Message);
    }

    private static ServiceOrder CreateOrder()
        => new("tenant-1", "SO-001", Guid.NewGuid(), Guid.NewGuid(), 15000, "Periyodik bakim");
}

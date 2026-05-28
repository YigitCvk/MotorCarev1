using MotorCare.Application.ServiceOrders.Commands.AddOperationToOrder;

namespace MotorCare.Application.UnitTests.ServiceOrders;

public class AddOperationToOrderCommandValidatorTests
{
    private readonly AddOperationToOrderCommandValidator _validator = new();

    private static AddOperationToOrderCommand ValidCommand() =>
        new(Guid.NewGuid(), "Yag degisimi", Quantity: 1m, UnitPrice: 100m);

    [Fact]
    public void Validate_ReturnsError_WhenQuantityIsZero()
    {
        var command = ValidCommand() with { Quantity = 0m };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Quantity));
    }

    [Fact]
    public void Validate_ReturnsError_WhenDescriptionIsEmpty()
    {
        var command = ValidCommand() with { Description = "" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Description));
    }

    [Fact]
    public void Validate_ReturnsError_WhenUnitPriceIsNegative()
    {
        var command = ValidCommand() with { UnitPrice = -1m };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.UnitPrice));
    }

    [Fact]
    public void Validate_ReturnsError_WhenDiscountIsNegative()
    {
        var command = ValidCommand() with { Discount = -1m };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Discount));
    }

    [Fact]
    public void Validate_ReturnsError_WhenDiscountExceedsLineTotal()
    {
        // Quantity=2, UnitPrice=100 => LineTotal=200. Discount=201 exceeds it.
        var command = ValidCommand() with { Quantity = 2m, UnitPrice = 100m, Discount = 201m };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Discount));
    }

    [Fact]
    public void Validate_IsValid_WhenAllFieldsAreCorrect()
    {
        var result = _validator.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
    }
}

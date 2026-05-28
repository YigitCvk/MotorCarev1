using MotorCare.Application.ServiceOrders.Commands.AddConsumableToOrder;

namespace MotorCare.Application.UnitTests.ServiceOrders;

public class AddConsumableToOrderCommandValidatorTests
{
    private readonly AddConsumableToOrderCommandValidator _validator = new();

    private static AddConsumableToOrderCommand ValidCommand() =>
        new(Guid.NewGuid(), "Motor Yagi", "10W-40", 75m, 2);

    [Fact]
    public void Validate_ReturnsError_WhenCategoryIsEmpty()
    {
        var command = ValidCommand() with { Category = "" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Category));
    }

    [Fact]
    public void Validate_ReturnsError_WhenProductNameIsEmpty()
    {
        var command = ValidCommand() with { ProductName = "" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.ProductName));
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
    public void Validate_ReturnsError_WhenQuantityIsZero()
    {
        var command = ValidCommand() with { Quantity = 0 };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Quantity));
    }

    [Fact]
    public void Validate_IsValid_WhenAllFieldsAreCorrect()
    {
        var result = _validator.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
    }
}

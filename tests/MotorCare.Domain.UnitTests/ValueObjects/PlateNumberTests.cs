using MotorCare.Domain.ValueObjects;

namespace MotorCare.Domain.UnitTests.ValueObjects;

public class PlateNumberTests
{
    [Theory]
    [InlineData(" 34 ABC 123 ", "34ABC123")]
    [InlineData("34-abc-123", "34ABC123")]
    [InlineData("34.abc.123", "34ABC123")]
    [InlineData("34abc123", "34ABC123")]
    public void Create_ShouldNormalizePlateNumberCorrectly_WhenGivenValidInput(string input, string expectedNormalized)
    {
        // Act
        var plateNumber = PlateNumber.Create(input);

        // Assert
        Assert.Equal(input, plateNumber.OriginalValue);
        Assert.Equal(expectedNormalized, plateNumber.NormalizedValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowArgumentException_WhenGivenEmptyString(string input)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => PlateNumber.Create(input));
    }

    [Theory]
    [InlineData("---")]
    [InlineData("...")]
    [InlineData("   -   ")]
    public void Create_ShouldThrowArgumentException_WhenGivenOnlyNonAlphanumericCharacters(string input)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => PlateNumber.Create(input));
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenNormalizedValuesAreSame()
    {
        // Arrange
        var plate1 = PlateNumber.Create("34 ABC 123");
        var plate2 = PlateNumber.Create("34-abc-123");

        // Act & Assert
        Assert.True(plate1.Equals(plate2));
        Assert.Equal(plate1.GetHashCode(), plate2.GetHashCode());
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenNormalizedValuesAreDifferent()
    {
        // Arrange
        var plate1 = PlateNumber.Create("34 ABC 123");
        var plate2 = PlateNumber.Create("34 ABC 124");

        // Act & Assert
        Assert.False(plate1.Equals(plate2));
        Assert.NotEqual(plate1.GetHashCode(), plate2.GetHashCode());
    }
}

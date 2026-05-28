using MotorCare.Infrastructure.Email;

namespace MotorCare.Application.UnitTests.Auth;

public class SecurityTokenFactoryTests
{
    private readonly SecurityTokenFactory _factory = new();

    [Fact]
    public void GenerateNumericCode_Returns6DigitString()
    {
        var code = _factory.GenerateNumericCode();

        Assert.Equal(6, code.Length);
    }

    [Fact]
    public void GenerateNumericCode_ReturnsOnlyDigits()
    {
        var code = _factory.GenerateNumericCode();

        Assert.Matches("^[0-9]{6}$", code);
    }

    [Fact]
    public void GenerateNumericCode_IsWithinRange()
    {
        var code = _factory.GenerateNumericCode();
        var value = int.Parse(code);

        value.Should().BeInRange(0, 999999);
    }

    [Fact]
    public void GenerateNumericCode_GeneratesVariousCodes_OverMultipleRuns()
    {
        // Collision probability is 1/1_000_000 per pair — 100 runs will almost never collide.
        var codes = Enumerable.Range(0, 100).Select(_ => _factory.GenerateNumericCode()).ToHashSet();

        codes.Should().HaveCountGreaterThan(1, "cryptographically random codes should not all be identical");
    }

    [Fact]
    public void Hash_ReturnsDeterministicValue()
    {
        var hash1 = _factory.Hash("123456");
        var hash2 = _factory.Hash("123456");

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Hash_DoesNotReturnPlaintext()
    {
        var code = "123456";
        var hash = _factory.Hash(code);

        hash.Should().NotBe(code);
        hash.Length.Should().BeGreaterThan(code.Length);
    }

    [Fact]
    public void Hash_DifferentInputsProduceDifferentHashes()
    {
        var h1 = _factory.Hash("123456");
        var h2 = _factory.Hash("654321");

        h1.Should().NotBe(h2);
    }
}

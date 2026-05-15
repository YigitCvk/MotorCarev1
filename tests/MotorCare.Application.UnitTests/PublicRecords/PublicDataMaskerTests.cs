using MotorCare.Application.PublicRecords;

namespace MotorCare.Application.UnitTests.PublicRecords;

public class PublicDataMaskerTests
{
    [Fact]
    public void MaskDisplayName_MasksEachNamePart()
    {
        PublicDataMasker.MaskDisplayName("Ahmet Yılmaz").Should().Be("A**** Y*****");
    }

    [Fact]
    public void MaskDisplayName_MasksSingleName()
    {
        PublicDataMasker.MaskDisplayName("Mehmet").Should().Be("M*****");
    }

    [Fact]
    public void MaskPhone_PreservesOnlyPrefixAndSuffix()
    {
        PublicDataMasker.MaskPhone("05321234567").Should().Be("0532 *** ** 67");
    }

    [Fact]
    public void MaskEmail_PreservesOnlyFirstCharacterAndDomain()
    {
        PublicDataMasker.MaskEmail("test@example.com").Should().Be("t***@example.com");
    }
}

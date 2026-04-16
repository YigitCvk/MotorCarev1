using MotorCare.Domain.Common;

namespace MotorCare.Domain.ValueObjects;

public class PlateNumber : ValueObject
{
    public string OriginalValue { get; private set; }
    public string NormalizedValue { get; private set; }

    private PlateNumber(string originalValue, string normalizedValue)
    {
        OriginalValue = originalValue;
        NormalizedValue = normalizedValue;
    }

    public static PlateNumber Create(string plate)
    {
        if (string.IsNullOrWhiteSpace(plate))
            throw new ArgumentException("Plate number cannot be empty.", nameof(plate));

        // Format rules: remove spaces, periods, dashes, to uppercase
        var normalized = new string(plate.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToUpperInvariant();
        
        if (string.IsNullOrEmpty(normalized))
            throw new ArgumentException("Invalid plate number formatting.", nameof(plate));

        return new PlateNumber(plate, normalized);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return NormalizedValue;
    }
}

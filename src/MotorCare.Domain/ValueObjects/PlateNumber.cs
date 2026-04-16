namespace MotorCare.Domain.ValueObjects;

public class PlateNumber
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

    public override bool Equals(object? obj)
    {
        if (obj is PlateNumber other)
            return NormalizedValue == other.NormalizedValue;
        return false;
    }

    public override int GetHashCode() => NormalizedValue.GetHashCode();
}

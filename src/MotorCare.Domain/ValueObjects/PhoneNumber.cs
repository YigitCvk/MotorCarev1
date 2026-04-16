using MotorCare.Domain.Common;

namespace MotorCare.Domain.ValueObjects;

public class PhoneNumber : ValueObject
{
    public string Value { get; private set; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty.", nameof(phoneNumber));

        var normalized = new string(phoneNumber.Where(char.IsDigit).ToArray());
        
        if (normalized.Length < 10)
            throw new ArgumentException("Invalid phone number length.", nameof(phoneNumber));

        return new PhoneNumber(normalized);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}

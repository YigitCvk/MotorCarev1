namespace MotorCare.Application.PublicRecords;

public static class PublicDataMasker
{
    public static string? MaskDisplayName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return null;
        }

        var parts = fullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(MaskNamePart)
            .ToList();

        return parts.Count == 0 ? null : string.Join(' ', parts);
    }

    public static string? MaskPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.Length < 6)
        {
            return new string('*', digits.Length);
        }

        var prefix = digits[..Math.Min(4, digits.Length)];
        var suffix = digits[^2..];
        return $"{prefix} *** ** {suffix}";
    }

    public static string? MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var parts = email.Trim().Split('@', 2);
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
        {
            return "***";
        }

        return $"{parts[0][0]}***@{parts[1]}";
    }

    private static string MaskNamePart(string value)
    {
        if (value.Length <= 1)
        {
            return "*";
        }

        return $"{value[0]}{new string('*', value.Length - 1)}";
    }
}

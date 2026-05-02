namespace MotorCare.Application.ServiceOrders;

public static class ServiceOrderAttachmentFileRules
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024;
    public const long MaxMultipartBodySizeBytes = MaxFileSizeBytes + 128 * 1024;
    public const int MaxFileNameLength = 260;
    public const int MaxDescriptionLength = 500;

    private static readonly IReadOnlyDictionary<string, string[]> ContentTypesByExtension =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = ["image/jpeg"],
            [".jpeg"] = ["image/jpeg"],
            [".png"] = ["image/png"],
            [".webp"] = ["image/webp"],
            [".pdf"] = ["application/pdf"]
        };

    public static bool IsAllowedExtension(string fileName)
        => ContentTypesByExtension.ContainsKey(GetExtension(fileName));

    public static bool IsAllowedContentType(string contentType)
        => ContentTypesByExtension.Values.Any(values => values.Contains(contentType, StringComparer.OrdinalIgnoreCase));

    public static bool IsAllowedExtensionContentTypePair(string fileName, string contentType)
    {
        var extension = GetExtension(fileName);
        return ContentTypesByExtension.TryGetValue(extension, out var allowedContentTypes) &&
               allowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase);
    }

    public static string GetExtension(string fileName)
        => Path.GetExtension(fileName)?.ToLowerInvariant() ?? string.Empty;
}

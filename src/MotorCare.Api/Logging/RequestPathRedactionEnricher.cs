using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace MotorCare.Api.Logging;

public sealed class RequestPathRedactionEnricher : ILogEventEnricher
{
    private static readonly string[] PathPropertyNames = ["RequestPath", "Path"];

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        foreach (var propertyName in PathPropertyNames)
        {
            if (!logEvent.Properties.TryGetValue(propertyName, out var value))
            {
                continue;
            }

            var path = ToStringValue(value);
            var redactedPath = RequestPathRedactor.Redact(path);
            if (string.Equals(path, redactedPath, StringComparison.Ordinal))
            {
                continue;
            }

            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(propertyName, redactedPath));
        }
    }

    private static string ToStringValue(LogEventPropertyValue value)
    {
        if (value is ScalarValue { Value: string text })
        {
            return text;
        }

        if (value is ScalarValue { Value: PathString pathString })
        {
            return pathString.Value ?? string.Empty;
        }

        if (value is ScalarValue { Value: not null } scalar)
        {
            return scalar.Value.ToString() ?? string.Empty;
        }

        var rendered = value.ToString();
        return rendered.Length >= 2 && rendered[0] == '"' && rendered[^1] == '"'
            ? rendered[1..^1]
            : rendered;
    }
}

public static class RequestPathRedactor
{
    private const string ApiServiceRecordPrefix = "/api/public/service-record/";
    private const string ApiInspectionReportPrefix = "/api/public/inspection-report/";
    private const string AppServiceRecordPrefix = "/public/service-record/";
    private const string AppInspectionReportPrefix = "/public/inspection-report/";

    public static string Redact(PathString path)
        => Redact(path.Value);

    public static string Redact(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path ?? string.Empty;
        }

        var queryIndex = path.IndexOf('?');
        var pathOnly = queryIndex >= 0 ? path[..queryIndex] : path;
        var query = queryIndex >= 0 ? path[queryIndex..] : string.Empty;

        return TryRedact(pathOnly, ApiServiceRecordPrefix, "/api/public/service-record/{slug}") ??
               TryRedact(pathOnly, ApiInspectionReportPrefix, "/api/public/inspection-report/{slug}") ??
               TryRedact(pathOnly, AppServiceRecordPrefix, "/public/service-record/{slug}") ??
               TryRedact(pathOnly, AppInspectionReportPrefix, "/public/inspection-report/{slug}") ??
               path;

        string? TryRedact(string currentPath, string prefix, string replacement)
        {
            if (!currentPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var slug = currentPath[prefix.Length..];
            if (string.IsNullOrWhiteSpace(slug) || slug.Contains('/'))
            {
                return null;
            }

            return replacement + query;
        }
    }
}

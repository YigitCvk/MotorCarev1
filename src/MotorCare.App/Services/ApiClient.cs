using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MotorCare.App.Services;

public sealed class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly TokenStorageService _tokenStorageService;

    public ApiClient(HttpClient httpClient, TokenStorageService tokenStorageService)
    {
        _httpClient = httpClient;
        _tokenStorageService = tokenStorageService;
    }

    public Task<TResponse?> GetAsync<TResponse>(string uri, bool authorized = true, CancellationToken cancellationToken = default)
    {
        return SendAsync<TResponse>(HttpMethod.Get, uri, null, authorized, cancellationToken);
    }

    public Task<TResponse?> PostAsync<TRequest, TResponse>(string uri, TRequest request, bool authorized = true, CancellationToken cancellationToken = default)
    {
        return SendAsync<TResponse>(HttpMethod.Post, uri, request, authorized, cancellationToken);
    }

    public Task PostAsync<TRequest>(string uri, TRequest request, bool authorized = true, CancellationToken cancellationToken = default)
    {
        return SendAsync<object>(HttpMethod.Post, uri, request, authorized, cancellationToken);
    }

    public Task PutAsync<TRequest>(string uri, TRequest request, bool authorized = true, CancellationToken cancellationToken = default)
    {
        return SendAsync<object>(HttpMethod.Put, uri, request, authorized, cancellationToken);
    }

    public Task<TResponse?> PutAsync<TRequest, TResponse>(string uri, TRequest request, bool authorized = true, CancellationToken cancellationToken = default)
    {
        return SendAsync<TResponse>(HttpMethod.Put, uri, request, authorized, cancellationToken);
    }

    public Task DeleteAsync(string uri, bool authorized = true, CancellationToken cancellationToken = default)
    {
        return SendAsync<object>(HttpMethod.Delete, uri, null, authorized, cancellationToken);
    }

    public Task PatchAsync<TRequest>(string uri, TRequest request, bool authorized = true, CancellationToken cancellationToken = default)
    {
        return SendAsync<object>(HttpMethod.Patch, uri, request, authorized, cancellationToken);
    }

    public Task<TResponse?> PatchAsync<TRequest, TResponse>(string uri, TRequest request, bool authorized = true, CancellationToken cancellationToken = default)
    {
        return SendAsync<TResponse>(HttpMethod.Patch, uri, request, authorized, cancellationToken);
    }

    public Task<TResponse?> PostMultipartAsync<TResponse>(
        string uri,
        MultipartFormDataContent content,
        bool authorized = true,
        CancellationToken cancellationToken = default)
    {
        return SendContentAsync<TResponse>(HttpMethod.Post, uri, content, authorized, cancellationToken);
    }

    public string ToAbsoluteApiUrl(string uri)
    {
        if (Uri.TryCreate(uri, UriKind.Absolute, out var absolute))
        {
            return absolute.ToString();
        }

        return new Uri(_httpClient.BaseAddress!, uri).ToString();
    }

    private async Task<TResponse?> SendAsync<TResponse>(
        HttpMethod method,
        string uri,
        object? request,
        bool authorized,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(method, uri);

        if (request is not null)
        {
            message.Content = JsonContent.Create(request);
        }

        if (authorized)
        {
            var accessToken = await _tokenStorageService.GetAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiExceptionAsync(response, uri, cancellationToken);
        }

        if (typeof(TResponse) == typeof(object) || response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
    }

    private async Task<TResponse?> SendContentAsync<TResponse>(
        HttpMethod method,
        string uri,
        HttpContent content,
        bool authorized,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(method, uri)
        {
            Content = content
        };

        if (authorized)
        {
            var accessToken = await _tokenStorageService.GetAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiExceptionAsync(response, uri, cancellationToken);
        }

        if (typeof(TResponse) == typeof(object) || response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
    }

    private static async Task<ApiException> CreateApiExceptionAsync(
        HttpResponseMessage response,
        string uri,
        CancellationToken cancellationToken)
    {
        var fallback = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Oturum süreniz dolmuş olabilir. Lütfen tekrar giriş yapın.",
            HttpStatusCode.Forbidden => "Bu işlem için yetkiniz bulunmuyor.",
            HttpStatusCode.NotFound => "İstenen kayıt bulunamadı.",
            HttpStatusCode.UnprocessableEntity => "Gönderilen bilgilerde bir hata var. Lütfen formu kontrol edin.",
            HttpStatusCode.Conflict => "Bu işlem mevcut kayıtlarla çakışıyor.",
            _ => "İşlem şu anda tamamlanamadı. Lütfen tekrar deneyin."
        };

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            return new ApiException(response.StatusCode, uri, fallback);
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            var builder = new StringBuilder();
            string? code = null;

            if (document.RootElement.TryGetProperty("code", out var codeEl))
            {
                code = codeEl.GetString();
            }

            // Prefer "message" (current format), fall back to "detail", then "title"
            if (document.RootElement.TryGetProperty("message", out var messageEl))
            {
                var msg = messageEl.GetString();
                if (!string.IsNullOrWhiteSpace(msg))
                    builder.Append(msg);
            }

            if (builder.Length == 0 && document.RootElement.TryGetProperty("detail", out var detail))
            {
                var msg = detail.GetString();
                if (!string.IsNullOrWhiteSpace(msg))
                    builder.Append(msg);
            }

            if (builder.Length == 0 && document.RootElement.TryGetProperty("title", out var title))
            {
                var msg = title.GetString();
                if (!string.IsNullOrWhiteSpace(msg))
                    builder.Append(msg);
            }

            if (document.RootElement.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in errors.EnumerateObject())
                {
                    if (property.Value.ValueKind != JsonValueKind.Array)
                        continue;

                    foreach (var error in property.Value.EnumerateArray())
                    {
                        var msg = error.GetString();
                        if (string.IsNullOrWhiteSpace(msg))
                            continue;

                        if (builder.Length > 0)
                            builder.Append(' ');

                        builder.Append(msg);
                    }
                }
            }

            var finalMessage = builder.Length > 0 ? builder.ToString() : fallback;
            return new ApiException(response.StatusCode, uri, finalMessage, code);
        }
        catch (JsonException)
        {
            // Never propagate raw server content — could contain SQL errors or stack traces
            return new ApiException(response.StatusCode, uri, fallback);
        }
    }
}

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

    private static async Task<ApiException> CreateApiExceptionAsync(
        HttpResponseMessage response,
        string uri,
        CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var fallback = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Oturum süreniz dolmuş olabilir. Lütfen tekrar giriş yapın.",
            HttpStatusCode.Forbidden => "Bu işlem için yetkiniz bulunmuyor.",
            _ => $"API isteği başarısız oldu ({(int)response.StatusCode})."
        };

        if (string.IsNullOrWhiteSpace(content))
        {
            return new ApiException(response.StatusCode, uri, fallback);
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            var builder = new StringBuilder();

            if (document.RootElement.TryGetProperty("detail", out var detail))
            {
                builder.Append(detail.GetString());
            }

            if (builder.Length == 0 && document.RootElement.TryGetProperty("title", out var title))
            {
                builder.Append(title.GetString());
            }

            if (document.RootElement.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in errors.EnumerateObject())
                {
                    if (property.Value.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    foreach (var error in property.Value.EnumerateArray())
                    {
                        var message = error.GetString();
                        if (string.IsNullOrWhiteSpace(message))
                        {
                            continue;
                        }

                        if (builder.Length > 0)
                        {
                            builder.Append(' ');
                        }

                        builder.Append(message);
                    }
                }
            }

            return new ApiException(response.StatusCode, uri, builder.Length > 0 ? builder.ToString() : content);
        }
        catch (JsonException)
        {
            return new ApiException(response.StatusCode, uri, content);
        }
    }
}

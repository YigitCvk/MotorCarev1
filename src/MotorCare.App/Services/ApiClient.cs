using System.Net.Http.Headers;
using System.Net.Http.Json;
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
            throw new InvalidOperationException(await CreateErrorMessageAsync(response, cancellationToken));
        }

        if (typeof(TResponse) == typeof(object) || response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
    }

    private static async Task<string> CreateErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            return $"API request failed with status {(int)response.StatusCode}.";
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.TryGetProperty("detail", out var detail))
            {
                return detail.GetString() ?? content;
            }

            if (document.RootElement.TryGetProperty("title", out var title))
            {
                return title.GetString() ?? content;
            }
        }
        catch (JsonException)
        {
            // Ignore and fall back to raw content.
        }

        return content;
    }
}

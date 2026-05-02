using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Components.Forms;
using MotorCare.App.Models.Common;
using MotorCare.App.Models.Customers;
using MotorCare.App.Models.ServiceOrders;
using MotorCare.App.Models.Vehicles;
using AttachmentFileRules = MotorCare.Application.ServiceOrders.ServiceOrderAttachmentFileRules;

namespace MotorCare.App.Services;

public sealed class ServiceOrdersService
{
    private readonly ApiClient _apiClient;

    public ServiceOrdersService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<PagedResult<ServiceOrderListItem>?> GetServiceOrdersAsync(
        string? searchText,
        string? status,
        DateTime? openedFrom,
        DateTime? openedTo,
        Guid? customerId = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();

        if (!string.IsNullOrWhiteSpace(searchText))
            query.Add($"q={Uri.EscapeDataString(searchText)}");

        if (!string.IsNullOrWhiteSpace(status))
            query.Add($"status={Uri.EscapeDataString(status)}");

        if (openedFrom.HasValue)
        {
            var startUtc = DateTime.SpecifyKind(openedFrom.Value.Date, DateTimeKind.Utc);
            query.Add($"openedFrom={Uri.EscapeDataString(new DateTimeOffset(startUtc).ToString("O", CultureInfo.InvariantCulture))}");
        }

        if (openedTo.HasValue)
        {
            var endUtc = DateTime.SpecifyKind(openedTo.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query.Add($"openedTo={Uri.EscapeDataString(new DateTimeOffset(endUtc).ToString("O", CultureInfo.InvariantCulture))}");
        }

        if (customerId.HasValue)
            query.Add($"customerId={customerId.Value}");

        query.Add($"pageNumber={pageNumber}");
        query.Add($"pageSize={pageSize}");

        var uri = "/api/service-orders?" + string.Join('&', query);
        return _apiClient.GetAsync<PagedResult<ServiceOrderListItem>>(uri, authorized: true, cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerLookupResponse>?> SearchCustomersAsync(string? searchText, CancellationToken cancellationToken = default)
    {
        var parts = new List<string> { "pageNumber=1", "pageSize=100" };
        if (!string.IsNullOrWhiteSpace(searchText))
            parts.Insert(0, $"q={Uri.EscapeDataString(searchText)}");

        var uri = "/api/customers?" + string.Join("&", parts);
        var paged = await _apiClient.GetAsync<PagedResult<CustomerLookupResponse>>(uri, authorized: true, cancellationToken);
        return paged?.Items;
    }

    public Task<VehicleLookupResponse?> GetVehicleByPlateAsync(string plate, CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<VehicleLookupResponse>($"/api/vehicles/{Uri.EscapeDataString(plate)}", authorized: true, cancellationToken);
    }

    public Task<Guid> CreateServiceOrderAsync(CreateServiceOrderRequest request, CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<CreateServiceOrderRequest, Guid>("/api/service-orders", request, authorized: true, cancellationToken)!;
    }

    public Task<IReadOnlyList<ConsumableCatalogItemDto>?> SearchConsumableCatalogAsync(
        string? query = null,
        string? category = null,
        int? maxResults = null,
        CancellationToken cancellationToken = default)
    {
        var uri = "/api/service-orders/consumable-catalog/search";
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(query))
            parts.Add($"query={Uri.EscapeDataString(query)}");
        
        if (!string.IsNullOrWhiteSpace(category))
            parts.Add($"category={Uri.EscapeDataString(category)}");
            
        if (maxResults.HasValue)
            parts.Add($"maxResults={maxResults.Value}");

        if (parts.Count > 0)
            uri += "?" + string.Join("&", parts);

        return _apiClient.GetAsync<IReadOnlyList<ConsumableCatalogItemDto>>(uri, authorized: true, cancellationToken);
    }

    public Task TrackConsumableCatalogUsageAsync(
        TrackConsumableCatalogUsageRequest request,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync("/api/service-orders/consumable-catalog/track", request, authorized: true, cancellationToken);
    }

    public Task<ServiceOrderDetail?> GetServiceOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<ServiceOrderDetail>($"/api/service-orders/{id}", authorized: true, cancellationToken);
    }

    public Task<IReadOnlyList<ServiceOrderStatusHistoryItem>?> GetStatusHistoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<IReadOnlyList<ServiceOrderStatusHistoryItem>>(
            $"/api/service-orders/{id}/status-history",
            authorized: true,
            cancellationToken);
    }

    public Task<IReadOnlyList<ServiceOrderActivityFeedItem>?> GetActivityFeedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<IReadOnlyList<ServiceOrderActivityFeedItem>>(
            $"/api/service-orders/{id}/activity-feed",
            authorized: true,
            cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceOrderAttachment>?> GetAttachmentsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var attachments = await _apiClient.GetAsync<IReadOnlyList<ServiceOrderAttachment>>(
            $"/api/service-orders/{id}/attachments",
            authorized: true,
            cancellationToken);

        if (attachments is null)
        {
            return null;
        }

        foreach (var attachment in attachments)
        {
            attachment.FileUrl = ToAttachmentProxyUrl(attachment.FileUrl);
        }

        return attachments;
    }

    public async Task<ServiceOrderAttachment?> UploadAttachmentAsync(
        Guid id,
        IBrowserFile file,
        string attachmentType,
        string? description,
        CancellationToken cancellationToken = default)
    {
        await using var stream = file.OpenReadStream(AttachmentFileRules.MaxFileSizeBytes, cancellationToken);
        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        content.Add(fileContent, "File", file.Name);
        content.Add(new StringContent(attachmentType), "AttachmentType");
        content.Add(new StringContent(description ?? string.Empty), "Description");

        var attachment = await _apiClient.PostMultipartAsync<ServiceOrderAttachment>(
            $"/api/service-orders/{id}/attachments",
            content,
            authorized: true,
            cancellationToken);

        if (attachment is not null)
        {
            attachment.FileUrl = ToAttachmentProxyUrl(attachment.FileUrl);
        }

        return attachment;
    }

    public Task DeleteAttachmentAsync(Guid id, Guid attachmentId, CancellationToken cancellationToken = default)
    {
        return _apiClient.DeleteAsync($"/api/service-orders/{id}/attachments/{attachmentId}", authorized: true, cancellationToken);
    }

    public Task AddOperationAsync(Guid id, AddOperationRequest request, CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync($"/api/service-orders/{id}/operations", request, authorized: true, cancellationToken);
    }

    public Task RemoveOperationAsync(Guid id, Guid operationId, CancellationToken cancellationToken = default)
    {
        return _apiClient.DeleteAsync($"/api/service-orders/{id}/operations/{operationId}", authorized: true, cancellationToken);
    }

    public Task AddPartAsync(Guid id, AddPartRequest request, CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync($"/api/service-orders/{id}/parts", request, authorized: true, cancellationToken);
    }

    public Task RemovePartAsync(Guid id, Guid partId, CancellationToken cancellationToken = default)
    {
        return _apiClient.DeleteAsync($"/api/service-orders/{id}/parts/{partId}", authorized: true, cancellationToken);
    }

    public Task AddPaymentAsync(Guid id, AddPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            amount = request.Amount,
            method = ToPaymentMethodValue(request.Method),
            paymentDate = request.PaymentDate?.ToUniversalTime()
        };

        return _apiClient.PostAsync($"/api/service-orders/{id}/payments", payload, authorized: true, cancellationToken);
    }

    public Task UpdateStatusAsync(Guid id, UpdateServiceOrderStatusRequest request, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            status = ToServiceOrderStatusValue(request.Status),
            note = request.Note
        };

        return _apiClient.PutAsync($"/api/service-orders/{id}/status", payload, authorized: true, cancellationToken);
    }

    private static int ToServiceOrderStatusValue(string status) => status switch
    {
        "Open" => 1,
        "InProgress" => 2,
        "WaitingForParts" => 3,
        "Completed" => 4,
        "Cancelled" => 5,
        "Delivered" => 6,
        _ => 2
    };

    private static int ToPaymentMethodValue(string method) => method switch
    {
        "Cash" => 1,
        "CreditCard" => 2,
        "BankTransfer" => 3,
        _ => 1
    };

    private static string ToAttachmentProxyUrl(string fileUrl)
    {
        const string apiPrefix = "/api/service-orders/";
        const string proxyPrefix = "/attachment-proxy/service-orders/";

        return fileUrl.StartsWith(apiPrefix, StringComparison.OrdinalIgnoreCase)
            ? proxyPrefix + fileUrl[apiPrefix.Length..]
            : fileUrl;
    }
}

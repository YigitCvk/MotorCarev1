using Microsoft.AspNetCore.Components.Forms;
using MotorCare.App.Models.Imports;

namespace MotorCare.App.Services;

public sealed class ImportsService
{
    private const string Base = "/api/imports";
    private readonly ApiClient _apiClient;

    public ImportsService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<List<ImportBatch>?> GetBatchesAsync(CancellationToken ct = default)
        => _apiClient.GetAsync<List<ImportBatch>>(Base, cancellationToken: ct);

    public Task<ImportBatch?> GetBatchAsync(Guid batchId, int previewRows = 50, CancellationToken ct = default)
        => _apiClient.GetAsync<ImportBatch>($"{Base}/{batchId}?previewRows={previewRows}", cancellationToken: ct);

    public Task<List<ImportBatchRow>?> GetRowsAsync(Guid batchId, string? status = null, int maxRows = 200, CancellationToken ct = default)
    {
        var url = $"{Base}/{batchId}/rows?maxRows={maxRows}";
        if (!string.IsNullOrEmpty(status)) url += $"&status={Uri.EscapeDataString(status)}";
        return _apiClient.GetAsync<List<ImportBatchRow>>(url, cancellationToken: ct);
    }

    public async Task<ImportBatch?> UploadAsync(IBrowserFile file, ImportType importType, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        var stream = file.OpenReadStream(maxAllowedSize: 6 * 1024 * 1024, cancellationToken: ct);
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", file.Name);

        var url = $"{Base}/upload?importType={importType}";
        return await _apiClient.PostMultipartAsync<ImportBatch>(url, content, cancellationToken: ct);
    }

    public Task<ImportBatch?> CommitAsync(Guid batchId, CancellationToken ct = default)
        => _apiClient.PostAsync<object, ImportBatch>($"{Base}/{batchId}/commit", new { }, cancellationToken: ct);

    public string GetTemplateUrl(ImportType importType)
        => _apiClient.ToAbsoluteApiUrl($"{Base}/templates/{importType}");
}

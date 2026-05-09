using MotorCare.App.Models.PublicRecords;

namespace MotorCare.App.Services;

public sealed class PublicRecordsService
{
    private readonly ApiClient _apiClient;

    public PublicRecordsService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<PublicRecordAccessResponse?> GetOrCreateForServiceOrderAsync(
        Guid serviceOrderId,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<object, PublicRecordAccessResponse>(
            $"/api/service-orders/{serviceOrderId:D}/public-access",
            new { },
            authorized: true,
            cancellationToken);
    }

    public Task<PublicRecordAccessResponse?> GetOrCreateForInspectionAsync(
        Guid inspectionId,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.PostAsync<object, PublicRecordAccessResponse>(
            $"/api/inspections/{inspectionId:D}/public-access",
            new { },
            authorized: true,
            cancellationToken);
    }

    public Task<PublicServiceRecordPreview?> GetServiceRecordPreviewAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<PublicServiceRecordPreview>(
            $"/api/public/service-record/{Uri.EscapeDataString(slug)}",
            authorized: false,
            cancellationToken);
    }

    public Task<PublicInspectionReportPreview?> GetInspectionReportPreviewAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<PublicInspectionReportPreview>(
            $"/api/public/inspection-report/{Uri.EscapeDataString(slug)}",
            authorized: false,
            cancellationToken);
    }
}

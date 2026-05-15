using MotorCare.App.Models.PublicRecords;

namespace MotorCare.App.Services;

public sealed class PublicRecordsService
{
    private readonly ApiClient _apiClient;

    public PublicRecordsService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<PublicRecordAccessResponse?> GetForServiceOrderAsync(
        Guid serviceOrderId,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<PublicRecordAccessResponse>(
            $"/api/service-orders/{serviceOrderId:D}/public-access",
            authorized: true,
            cancellationToken);
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

    public Task<PublicRecordAccessResponse?> EnableForServiceOrderAsync(
        Guid serviceOrderId,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.PutAsync<object, PublicRecordAccessResponse>(
            $"/api/service-orders/{serviceOrderId:D}/public-access/enable",
            new { },
            authorized: true,
            cancellationToken);
    }

    public Task DisableForServiceOrderAsync(
        Guid serviceOrderId,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.PutAsync(
            $"/api/service-orders/{serviceOrderId:D}/public-access/disable",
            new { },
            authorized: true,
            cancellationToken);
    }

    public Task<PublicRecordAccessResponse?> GetForInspectionAsync(
        Guid inspectionId,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<PublicRecordAccessResponse>(
            $"/api/inspections/{inspectionId:D}/public-access",
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

    public Task<PublicRecordAccessResponse?> EnableForInspectionAsync(
        Guid inspectionId,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.PutAsync<object, PublicRecordAccessResponse>(
            $"/api/inspections/{inspectionId:D}/public-access/enable",
            new { },
            authorized: true,
            cancellationToken);
    }

    public Task DisableForInspectionAsync(
        Guid inspectionId,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.PutAsync(
            $"/api/inspections/{inspectionId:D}/public-access/disable",
            new { },
            authorized: true,
            cancellationToken);
    }

    public Task<PublicServiceRecord?> GetServiceRecordAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<PublicServiceRecord>(
            $"/api/public/service-record/{Uri.EscapeDataString(slug)}",
            authorized: false,
            cancellationToken);
    }

    public Task<PublicInspectionReport?> GetInspectionReportAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return _apiClient.GetAsync<PublicInspectionReport>(
            $"/api/public/inspection-report/{Uri.EscapeDataString(slug)}",
            authorized: false,
            cancellationToken);
    }
}

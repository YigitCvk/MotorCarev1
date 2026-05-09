using MotorCare.Application.PublicRecords;
using MotorCare.Domain.PublicRecords;

namespace MotorCare.Application.Common.Interfaces;

public interface IPublicRecordAccessService
{
    Task<PublicRecordAccessDto?> GetOrCreateForServiceOrderAsync(Guid serviceOrderId, string tenantId, CancellationToken cancellationToken = default);

    Task<PublicRecordAccessDto?> GetOrCreateForInspectionAsync(Guid inspectionId, string tenantId, CancellationToken cancellationToken = default);

    Task<PublicRecordAccessDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task DisableAsync(PublicRecordType recordType, Guid recordId, string tenantId, CancellationToken cancellationToken = default);

    Task<PublicServiceRecordPreviewDto?> GetServiceRecordPreviewAsync(string slug, CancellationToken cancellationToken = default);

    Task<PublicInspectionReportPreviewDto?> GetInspectionReportPreviewAsync(string slug, CancellationToken cancellationToken = default);
}

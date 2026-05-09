using MotorCare.Domain.PublicRecords;

namespace MotorCare.Application.PublicRecords;

public sealed record PublicRecordAccessDto(
    string TenantId,
    PublicRecordType RecordType,
    Guid RecordId,
    string Slug,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastAccessedAtUtc,
    int AccessCount);

public sealed record PublicServiceRecordPreviewDto(
    string OrderNo,
    DateTimeOffset Date,
    string? VehiclePlate,
    string? VehicleBrand,
    string? VehicleModel,
    string ServiceSummary,
    string Status,
    string? BusinessName,
    string VerificationText);

public sealed record PublicInspectionReportPreviewDto(
    string InspectionNo,
    DateTimeOffset Date,
    string VehiclePlate,
    string? VehicleBrand,
    string? VehicleModel,
    string PackageType,
    string Status,
    bool IsCompleted,
    int CriticalFindingCount,
    string ResultSummary,
    string? BusinessName,
    string VerificationText);

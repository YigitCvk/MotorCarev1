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

public sealed record PublicServiceRecordOperationDto(
    string Description);

public sealed record PublicServiceRecordPartDto(
    string PartName,
    string? PartNumber,
    int Quantity);

public sealed record PublicServiceRecordConsumableDto(
    string Category,
    string Brand,
    string ProductName,
    string? SubCategory,
    string? Specification,
    string? Notes);

public sealed record PublicServiceRecordTotalsDto(
    decimal LaborTotal,
    decimal PartsTotal,
    decimal DiscountTotal,
    decimal GrandTotal,
    decimal PaidTotal,
    decimal RemainingTotal);

public sealed record PublicServiceRecordPaymentSummaryDto(
    int PaymentCount,
    decimal TotalPaid,
    decimal CashTotal,
    decimal CreditCardTotal,
    decimal BankTransferTotal);

public sealed record PublicServiceRecordPaymentDto(
    decimal Amount,
    string Method,
    DateTimeOffset PaymentDate);

public sealed record PublicServiceRecordDto(
    string OrderNo,
    DateTimeOffset Date,
    string? MaskedCustomerDisplayName,
    string? VehiclePlate,
    string? VehicleBrand,
    string? VehicleModel,
    int VehicleKm,
    string? WorkDescription,
    string ServiceSummary,
    string Status,
    DateTimeOffset? ClosedAt,
    IReadOnlyList<PublicServiceRecordOperationDto> Operations,
    IReadOnlyList<PublicServiceRecordPartDto> Parts,
    IReadOnlyList<PublicServiceRecordConsumableDto> Consumables,
    PublicServiceRecordTotalsDto Totals,
    PublicServiceRecordPaymentSummaryDto PaymentSummary,
    IReadOnlyList<PublicServiceRecordPaymentDto> Payments,
    string? BusinessName,
    string VerificationText);

public sealed record PublicInspectionReportItemDto(
    string Category,
    string Name,
    string Result,
    string? Notes,
    int SortOrder);

public sealed record PublicInspectionReportDto(
    string InspectionNo,
    DateTimeOffset Date,
    string VehiclePlate,
    string? VehicleBrand,
    string? VehicleModel,
    int? VehicleYear,
    int? VehicleMileage,
    string PackageType,
    string Status,
    bool IsCompleted,
    int CriticalFindingCount,
    string ResultSummary,
    string? GeneralNotes,
    string? TestRideNotes,
    string? CosmeticNotes,
    IReadOnlyList<PublicInspectionReportItemDto> Items,
    string? BusinessName,
    string VerificationText);

using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.Inspections;
using MotorCare.Application.PublicRecords;
using MotorCare.Domain.Enums;
using MotorCare.Domain.PublicRecords;
using MotorCare.Infrastructure.Persistence;

namespace MotorCare.Infrastructure.Services;

public sealed class PublicRecordAccessService : IPublicRecordAccessService
{
    private const int SlugByteLength = 24;
    private const int MaxSlugAttempts = 8;
    private const string VerificationText = "Bu kayıt GarajPass üzerinden doğrulanmıştır.";

    private readonly ApplicationDbContext _context;

    public PublicRecordAccessService(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<PublicRecordAccessDto?> GetOrCreateForServiceOrderAsync(
        Guid serviceOrderId,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        return GetOrCreateAsync(PublicRecordType.ServiceOrder, serviceOrderId, tenantId, cancellationToken);
    }

    public Task<PublicRecordAccessDto?> GetOrCreateForInspectionAsync(
        Guid inspectionId,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        return GetOrCreateAsync(PublicRecordType.MotorcycleInspection, inspectionId, tenantId, cancellationToken);
    }

    public async Task<PublicRecordAccessDto?> GetForRecordAsync(
        PublicRecordType recordType,
        Guid recordId,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var normalizedTenantId = NormalizeTenantId(tenantId);
        if (string.IsNullOrWhiteSpace(normalizedTenantId) || recordId == Guid.Empty)
        {
            return null;
        }

        var access = await _context.PublicRecordAccesses
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == normalizedTenantId && x.RecordType == recordType && x.RecordId == recordId,
                cancellationToken);

        return access is null ? null : ToDto(access);
    }

    public async Task<PublicRecordAccessDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = NormalizeSlug(slug);
        if (string.IsNullOrWhiteSpace(normalizedSlug))
        {
            return null;
        }

        var access = await _context.PublicRecordAccesses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == normalizedSlug && x.IsActive, cancellationToken);

        return access is null ? null : ToDto(access);
    }

    public async Task<PublicRecordAccessDto?> EnableAsync(
        PublicRecordType recordType,
        Guid recordId,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var normalizedTenantId = NormalizeTenantId(tenantId);
        if (string.IsNullOrWhiteSpace(normalizedTenantId) || recordId == Guid.Empty)
        {
            return null;
        }

        if (!await RecordExistsAsync(recordType, recordId, normalizedTenantId, cancellationToken))
        {
            return null;
        }

        var existing = await _context.PublicRecordAccesses
            .FirstOrDefaultAsync(
                x => x.TenantId == normalizedTenantId && x.RecordType == recordType && x.RecordId == recordId,
                cancellationToken);

        if (existing is not null)
        {
            existing.Enable();
            await _context.SaveChangesAsync(cancellationToken);
            return ToDto(existing);
        }

        return await CreateAsync(recordType, recordId, normalizedTenantId, cancellationToken);
    }

    public async Task DisableAsync(
        PublicRecordType recordType,
        Guid recordId,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var normalizedTenantId = NormalizeTenantId(tenantId);
        if (string.IsNullOrWhiteSpace(normalizedTenantId) || recordId == Guid.Empty)
        {
            return;
        }

        var access = await _context.PublicRecordAccesses
            .FirstOrDefaultAsync(
                x => x.TenantId == normalizedTenantId && x.RecordType == recordType && x.RecordId == recordId,
                cancellationToken);

        if (access is null)
        {
            return;
        }

        access.Disable(DateTimeOffset.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PublicServiceRecordDto?> GetServiceRecordAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var access = await GetActiveAccessForPublicReadAsync(slug, PublicRecordType.ServiceOrder, cancellationToken);
        if (access is null)
        {
            return null;
        }

        var order = await _context.ServiceOrders
            .IgnoreQueryFilters()
            .AsSplitQuery()
            .AsNoTracking()
            .Include(x => x.Operations)
            .Include(x => x.Parts)
            .Include(x => x.Consumables)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(
                x => x.Id == access.RecordId && x.TenantId == access.TenantId,
                cancellationToken);

        if (order is null)
        {
            return null;
        }

        var vehicle = await _context.Vehicles
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == order.VehicleId && x.TenantId == access.TenantId,
                cancellationToken);

        var customerName = await _context.Customers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == order.CustomerId && x.TenantId == access.TenantId)
            .Select(x => x.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        var tenantName = await _context.Tenants
            .AsNoTracking()
            .Where(x => x.Identifier == access.TenantId && x.IsActive)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);

        var payments = order.Payments
            .OrderBy(x => x.PaymentDate)
            .ThenBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .ToList();

        return new PublicServiceRecordDto(
            order.OrderNo,
            order.OpenedAt,
            PublicDataMasker.MaskDisplayName(customerName),
            vehicle?.Plate.OriginalValue,
            vehicle?.Brand,
            vehicle?.Model,
            order.VehicleKm,
            order.WorkDescription,
            BuildServiceSummary(order.WorkDescription, order.Operations.Select(x => x.Description)),
            order.Status.ToString(),
            order.ClosedAt,
            order.Operations
                .OrderBy(x => x.CreatedAt)
                .ThenBy(x => x.Id)
                .Select(x => new PublicServiceRecordOperationDto(x.Description))
                .ToList(),
            order.Parts
                .OrderBy(x => x.CreatedAt)
                .ThenBy(x => x.Id)
                .Select(x => new PublicServiceRecordPartDto(x.PartName, x.PartNumber, x.Quantity))
                .ToList(),
            order.Consumables
                .OrderBy(x => x.CreatedAt)
                .ThenBy(x => x.Id)
                .Select(x => new PublicServiceRecordConsumableDto(
                    x.Category,
                    x.Brand,
                    x.ProductName,
                    x.SubCategory,
                    x.Specification,
                    x.Notes))
                .ToList(),
            new PublicServiceRecordTotalsDto(
                order.LaborTotal,
                order.PartsTotal,
                order.DiscountTotal,
                order.GrandTotal,
                order.PaidTotal,
                order.RemainingTotal),
            new PublicServiceRecordPaymentSummaryDto(
                payments.Count,
                order.PaidTotal,
                payments.Where(x => x.Method == PaymentMethod.Cash).Sum(x => x.Amount),
                payments.Where(x => x.Method == PaymentMethod.CreditCard).Sum(x => x.Amount),
                payments.Where(x => x.Method == PaymentMethod.BankTransfer).Sum(x => x.Amount)),
            payments
                .Select(x => new PublicServiceRecordPaymentDto(
                    x.Amount,
                    x.Method.ToString(),
                    x.PaymentDate))
                .ToList(),
            tenantName,
            VerificationText);
    }

    public async Task<PublicInspectionReportDto?> GetInspectionReportAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var access = await GetActiveAccessForPublicReadAsync(slug, PublicRecordType.MotorcycleInspection, cancellationToken);
        if (access is null)
        {
            return null;
        }

        var inspection = await _context.MotorcycleInspections
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(
                x => x.Id == access.RecordId && x.TenantId == access.TenantId,
                cancellationToken);

        if (inspection is null)
        {
            return null;
        }

        var tenantName = await _context.Tenants
            .AsNoTracking()
            .Where(x => x.Identifier == access.TenantId && x.IsActive)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);

        var criticalCount = inspection.Items.Count(x =>
            x.Result is MotorcycleInspectionResult.Damaged or MotorcycleInspectionResult.Changed);

        return new PublicInspectionReportDto(
            inspection.InspectionNo,
            inspection.CompletedAt ?? inspection.CreatedAt,
            inspection.Plate,
            inspection.Brand,
            inspection.Model,
            inspection.Year,
            inspection.Mileage,
            MotorcycleInspectionTextMapper.ToText(inspection.PackageType),
            MotorcycleInspectionTextMapper.ToText(inspection.Status),
            inspection.Status == MotorcycleInspectionStatus.Completed,
            criticalCount,
            BuildInspectionSummary(inspection.Status, criticalCount),
            inspection.GeneralNotes,
            inspection.TestRideNotes,
            inspection.CosmeticNotes,
            inspection.Items
                .OrderBy(x => x.Category)
                .ThenBy(x => x.SortOrder)
                .Select(x => new PublicInspectionReportItemDto(
                    MotorcycleInspectionTextMapper.ToText(x.Category),
                    x.Name,
                    MotorcycleInspectionTextMapper.ToText(x.Result),
                    x.Notes,
                    x.SortOrder))
                .ToList(),
            tenantName,
            VerificationText);
    }

    private async Task<PublicRecordAccessDto?> GetOrCreateAsync(
        PublicRecordType recordType,
        Guid recordId,
        string tenantId,
        CancellationToken cancellationToken)
    {
        var normalizedTenantId = NormalizeTenantId(tenantId);
        if (string.IsNullOrWhiteSpace(normalizedTenantId) || recordId == Guid.Empty)
        {
            return null;
        }

        if (!await RecordExistsAsync(recordType, recordId, normalizedTenantId, cancellationToken))
        {
            return null;
        }

        var existing = await _context.PublicRecordAccesses
            .FirstOrDefaultAsync(
                x => x.TenantId == normalizedTenantId && x.RecordType == recordType && x.RecordId == recordId,
                cancellationToken);

        if (existing is not null)
        {
            return ToDto(existing);
        }

        return await CreateAsync(recordType, recordId, normalizedTenantId, cancellationToken);
    }

    private async Task<PublicRecordAccessDto> CreateAsync(
        PublicRecordType recordType,
        Guid recordId,
        string tenantId,
        CancellationToken cancellationToken)
    {
        PublicRecordAccess? existing;

        for (var attempt = 0; attempt < MaxSlugAttempts; attempt++)
        {
            var slug = GenerateSlug();
            var slugExists = await _context.PublicRecordAccesses
                .AsNoTracking()
                .AnyAsync(x => x.Slug == slug, cancellationToken);

            if (slugExists)
            {
                continue;
            }

            var access = new PublicRecordAccess(
                tenantId,
                recordType,
                recordId,
                slug,
                DateTimeOffset.UtcNow);

            await _context.PublicRecordAccesses.AddAsync(access, cancellationToken);
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return ToDto(access);
            }
            catch (DbUpdateException) when (attempt < MaxSlugAttempts - 1)
            {
                _context.Entry(access).State = EntityState.Detached;

                existing = await _context.PublicRecordAccesses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.TenantId == tenantId && x.RecordType == recordType && x.RecordId == recordId,
                        cancellationToken);

                if (existing is not null)
                {
                    return ToDto(existing);
                }
            }
        }

        throw new InvalidOperationException("Could not generate a unique public record slug.");
    }

    private Task<bool> RecordExistsAsync(
        PublicRecordType recordType,
        Guid recordId,
        string tenantId,
        CancellationToken cancellationToken)
    {
        return recordType switch
        {
            PublicRecordType.ServiceOrder => _context.ServiceOrders
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(x => x.Id == recordId && x.TenantId == tenantId, cancellationToken),
            PublicRecordType.MotorcycleInspection => _context.MotorcycleInspections
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(x => x.Id == recordId && x.TenantId == tenantId, cancellationToken),
            _ => Task.FromResult(false)
        };
    }

    private async Task<PublicRecordAccess?> GetActiveAccessForPublicReadAsync(
        string slug,
        PublicRecordType expectedRecordType,
        CancellationToken cancellationToken)
    {
        var normalizedSlug = NormalizeSlug(slug);
        if (string.IsNullOrWhiteSpace(normalizedSlug))
        {
            return null;
        }

        return await _context.PublicRecordAccesses
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Slug == normalizedSlug &&
                     x.IsActive &&
                     x.RecordType == expectedRecordType,
                cancellationToken);
    }

    private static string GenerateSlug()
    {
        var bytes = RandomNumberGenerator.GetBytes(SlugByteLength);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static string? NormalizeTenantId(string? tenantId)
        => string.IsNullOrWhiteSpace(tenantId) ? null : tenantId.Trim();

    private static string? NormalizeSlug(string? slug)
        => string.IsNullOrWhiteSpace(slug) ? null : slug.Trim();

    private static PublicRecordAccessDto ToDto(PublicRecordAccess access)
        => new(
            access.TenantId,
            access.RecordType,
            access.RecordId,
            access.Slug,
            access.IsActive,
            access.CreatedAtUtc,
            access.LastAccessedAtUtc,
            access.AccessCount);

    private static string BuildServiceSummary(string? workDescription, IEnumerable<string> operationDescriptions)
    {
        if (!string.IsNullOrWhiteSpace(workDescription))
        {
            return Truncate(workDescription.Trim(), 240);
        }

        var operations = operationDescriptions
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Take(3)
            .ToList();

        return operations.Count == 0
            ? "Servis kaydı GarajPass üzerinden doğrulanmıştır."
            : Truncate(string.Join(", ", operations), 240);
    }

    private static string BuildInspectionSummary(MotorcycleInspectionStatus status, int criticalCount)
    {
        if (status != MotorcycleInspectionStatus.Completed)
        {
            return "Ekspertiz kaydı henüz tamamlanmadı.";
        }

        return criticalCount == 0
            ? "Public özette kritik bulgu işaretlenmedi."
            : $"Ekspertiz özetinde {criticalCount} kritik bulgu işaretlendi.";
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength].TrimEnd() + "...";
}

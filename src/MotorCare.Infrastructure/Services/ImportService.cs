using System.Globalization;
using System.Text;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MotorCare.Application.Common;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.Imports;
using MotorCare.Domain.Enums;
using MotorCare.Domain.Imports;
using MotorCare.Domain.ServiceOrders;
using MotorCare.Domain.ServiceOrders.Entities;
using MotorCare.Domain.ValueObjects;
using MotorCare.Domain.Vehicles;
using MotorCare.Infrastructure.Persistence;

namespace MotorCare.Infrastructure.Services;

public sealed class ImportService : IImportService
{
    private const int MaxFileSizeBytes = 5 * 1024 * 1024;
    private const int MaxRows = 10_000;
    private const int PreviewDefaultRows = 50;

    private static readonly string[] CustomerColumns =
        ["CustomerName", "Phone", "Email", "TaxNumber", "Address", "Notes"];

    private static readonly string[] VehicleColumns =
        ["CustomerPhone", "CustomerEmail", "Plate", "Brand", "Model", "Year",
         "VehicleType", "ChassisNumber", "EngineNumber", "Mileage", "Notes"];

    private static readonly string[] ServiceHistoryColumns =
        ["CustomerPhone", "CustomerEmail", "Plate", "ServiceDate", "Mileage",
         "ServiceTitle", "Description", "LaborDescription", "PartsDescription",
         "ConsumablesDescription", "TotalAmount", "PaidAmount", "Status", "Notes"];

    private readonly ApplicationDbContext _context;
    private readonly IOrderNumberGenerator _orderNumberGenerator;
    private readonly ILogger<ImportService> _logger;

    public ImportService(
        ApplicationDbContext context,
        IOrderNumberGenerator orderNumberGenerator,
        ILogger<ImportService> logger)
    {
        _context = context;
        _orderNumberGenerator = orderNumberGenerator;
        _logger = logger;
    }

    public async Task<ImportBatchDto> UploadAndParseAsync(
        Stream fileStream,
        string originalFileName,
        string contentType,
        ImportType importType,
        string tenantId,
        Guid? userId,
        CancellationToken cancellationToken = default)
    {
        if (fileStream.Length > MaxFileSizeBytes)
        {
            _logger.LogWarning(EventIdStore.Import.ImportFileTooLarge,
                "Import file too large. TenantId={TenantId} Size={Size} Limit={Limit}",
                tenantId, fileStream.Length, MaxFileSizeBytes);
            throw new InvalidOperationException($"Dosya boyutu 5 MB sınırını aşıyor ({fileStream.Length / 1024:N0} KB).");
        }

        var safeFileName = SanitizeFileName(originalFileName);
        var batch = new ImportBatch(tenantId, importType, safeFileName, originalFileName, contentType, userId);

        _logger.LogInformation(EventIdStore.Import.ImportUploaded,
            "Import uploaded. TenantId={TenantId} ImportType={ImportType} FileName={FileName}",
            tenantId, importType, safeFileName);

        List<Dictionary<string, string>> rawRows;
        try
        {
            rawRows = IsExcel(originalFileName)
                ? ParseExcel(fileStream)
                : ParseCsv(fileStream);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(EventIdStore.Import.ImportParseError,
                "Import parse failed. TenantId={TenantId} ImportType={ImportType} Error={Error}",
                tenantId, importType, ex.Message);
            batch.SetFailed();
            await _context.ImportBatches.AddAsync(batch, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException($"Dosya okunamadı: {ex.Message}");
        }

        if (rawRows.Count > MaxRows)
        {
            throw new InvalidOperationException($"Dosya {rawRows.Count:N0} satır içeriyor. Maksimum {MaxRows:N0} satır desteklenir.");
        }

        _logger.LogInformation(EventIdStore.Import.ImportParsed,
            "Import parsed. TenantId={TenantId} ImportType={ImportType} TotalRows={TotalRows}",
            tenantId, importType, rawRows.Count);

        var batchRows = new List<ImportBatchRow>(rawRows.Count);
        int rowNumber = 0;

        foreach (var raw in rawRows)
        {
            rowNumber++;
            var rawJson = JsonSerializer.Serialize(raw);
            var batchRow = new ImportBatchRow(batch.Id, tenantId, rowNumber, rawJson);

            await ValidateRowAsync(batchRow, raw, importType, tenantId, cancellationToken);
            batchRows.Add(batchRow);
        }

        batch.SetValidated(
            rowNumber,
            batchRows.Count(r => r.Status == ImportRowStatus.Valid),
            batchRows.Count(r => r.Status == ImportRowStatus.Warning),
            batchRows.Count(r => r.Status == ImportRowStatus.Error));

        _logger.LogInformation(EventIdStore.Import.ImportValidationDone,
            "Import validated. TenantId={TenantId} ImportType={ImportType} Valid={Valid} Warning={Warning} Error={Error}",
            tenantId, importType, batch.ValidRows, batch.WarningRows, batch.ErrorRows);

        await _context.ImportBatches.AddAsync(batch, cancellationToken);
        await _context.ImportBatchRows.AddRangeAsync(batchRows, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return ToDtoWithRows(batch, batchRows.Take(PreviewDefaultRows).ToList());
    }

    public async Task<ImportBatchDto?> GetBatchAsync(
        Guid batchId,
        string tenantId,
        int previewRows = PreviewDefaultRows,
        CancellationToken cancellationToken = default)
    {
        var batch = await _context.ImportBatches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == batchId && b.TenantId == tenantId, cancellationToken);

        if (batch is null) return null;

        var rows = await _context.ImportBatchRows
            .AsNoTracking()
            .Where(r => r.ImportBatchId == batchId)
            .OrderBy(r => r.RowNumber)
            .Take(previewRows)
            .ToListAsync(cancellationToken);

        return ToDtoWithRows(batch, rows);
    }

    public async Task<IReadOnlyList<ImportBatchDto>> GetBatchesAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var batches = await _context.ImportBatches
            .AsNoTracking()
            .Where(b => b.TenantId == tenantId)
            .OrderByDescending(b => b.CreatedAtUtc)
            .Take(50)
            .ToListAsync(cancellationToken);

        return batches.Select(b => ToDtoWithRows(b, [])).ToList();
    }

    public async Task<IReadOnlyList<ImportBatchRowDto>> GetRowsAsync(
        Guid batchId,
        string tenantId,
        ImportRowStatus? status = null,
        int maxRows = 200,
        CancellationToken cancellationToken = default)
    {
        var batchExists = await _context.ImportBatches
            .AsNoTracking()
            .AnyAsync(b => b.Id == batchId && b.TenantId == tenantId, cancellationToken);

        if (!batchExists) return [];

        var query = _context.ImportBatchRows
            .AsNoTracking()
            .Where(r => r.ImportBatchId == batchId);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        var rows = await query
            .OrderBy(r => r.RowNumber)
            .Take(maxRows)
            .ToListAsync(cancellationToken);

        return rows.Select(ToRowDto).ToList();
    }

    public async Task<ImportBatchDto> CommitAsync(
        Guid batchId,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var batch = await _context.ImportBatches
            .FirstOrDefaultAsync(b => b.Id == batchId && b.TenantId == tenantId, cancellationToken)
            ?? throw new InvalidOperationException($"Import batch '{batchId}' bulunamadı.");

        if (batch.Status == ImportBatchStatus.Imported)
        {
            var existingRows = await _context.ImportBatchRows
                .AsNoTracking()
                .Where(r => r.ImportBatchId == batchId)
                .Take(PreviewDefaultRows)
                .ToListAsync(cancellationToken);
            return ToDtoWithRows(batch, existingRows);
        }

        if (batch.Status is not (ImportBatchStatus.Validated or ImportBatchStatus.Failed))
        {
            throw new InvalidOperationException($"Batch '{batchId}' commit için hazır değil (durum: {batch.Status}).");
        }

        var rows = await _context.ImportBatchRows
            .Where(r => r.ImportBatchId == batchId &&
                        (r.Status == ImportRowStatus.Valid || r.Status == ImportRowStatus.Warning))
            .OrderBy(r => r.RowNumber)
            .ToListAsync(cancellationToken);

        int imported = 0, skipped = 0;

        foreach (var row in rows)
        {
            try
            {
                var wasImported = await CommitRowAsync(row, batch.ImportType, tenantId, cancellationToken);
                if (wasImported)
                {
                    row.SetImported();
                    imported++;
                }
                else
                {
                    row.SetSkipped("Duplicate kayıt, atlandı.");
                    skipped++;
                    _logger.LogInformation(EventIdStore.Import.ImportRowSkipped,
                        "Import row skipped. TenantId={TenantId} BatchId={BatchId} RowNumber={RowNumber}",
                        tenantId, batchId, row.RowNumber);
                }
            }
            catch (Exception ex)
            {
                row.SetError(ex.Message.Length > 500 ? ex.Message[..500] : ex.Message);
                _logger.LogWarning(EventIdStore.Import.ImportRowError,
                    "Import row error. TenantId={TenantId} BatchId={BatchId} RowNumber={RowNumber} Error={Error}",
                    tenantId, batchId, row.RowNumber, ex.Message);
            }
        }

        batch.SetImported(imported, skipped);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(EventIdStore.Import.ImportCommitted,
            "Import committed. TenantId={TenantId} BatchId={BatchId} ImportType={ImportType} Imported={Imported} Skipped={Skipped}",
            tenantId, batchId, batch.ImportType, imported, skipped);

        var previewRows = await _context.ImportBatchRows
            .AsNoTracking()
            .Where(r => r.ImportBatchId == batchId)
            .OrderBy(r => r.RowNumber)
            .Take(PreviewDefaultRows)
            .ToListAsync(cancellationToken);

        return ToDtoWithRows(batch, previewRows);
    }

    public (byte[] Content, string FileName, string ContentType) GetCsvTemplate(ImportType importType)
    {
        var columns = importType switch
        {
            ImportType.Customers => CustomerColumns,
            ImportType.Vehicles => VehicleColumns,
            ImportType.ServiceHistory => ServiceHistoryColumns,
            _ => throw new ArgumentOutOfRangeException(nameof(importType))
        };

        var fileName = importType switch
        {
            ImportType.Customers => "customers.csv",
            ImportType.Vehicles => "vehicles.csv",
            ImportType.ServiceHistory => "service-history.csv",
            _ => "template.csv"
        };

        using var ms = new MemoryStream();
        // UTF-8 BOM so Excel opens it correctly with Turkish characters
        ms.Write([0xEF, 0xBB, 0xBF]);
        using var writer = new StreamWriter(ms, new UTF8Encoding(false), leaveOpen: true);
        writer.WriteLine(string.Join(",", columns));
        writer.Flush();

        return (ms.ToArray(), fileName, "text/csv; charset=utf-8");
    }

    // ─── Parsing ────────────────────────────────────────────────────────────

    private static List<Dictionary<string, string>> ParseCsv(Stream stream)
    {
        // Reset if seekable
        if (stream.CanSeek) stream.Position = 0;

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null,
            TrimOptions = TrimOptions.Trim,
            Delimiter = DetectDelimiter(stream),
        };

        if (stream.CanSeek) stream.Position = 0;

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: -1, leaveOpen: true);
        using var csv = new CsvReader(reader, config);

        csv.Read();
        csv.ReadHeader();
        var headers = csv.HeaderRecord ?? throw new InvalidOperationException("CSV başlık satırı okunamadı.");

        var rows = new List<Dictionary<string, string>>();
        while (csv.Read())
        {
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in headers)
            {
                row[header.Trim()] = csv.GetField(header)?.Trim() ?? string.Empty;
            }
            rows.Add(row);
        }
        return rows;
    }

    private static string DetectDelimiter(Stream stream)
    {
        if (!stream.CanSeek) return ",";
        var pos = stream.Position;
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: -1, leaveOpen: true);
        var firstLine = reader.ReadLine() ?? string.Empty;
        stream.Position = pos;
        var semicolonCount = firstLine.Count(c => c == ';');
        var commaCount = firstLine.Count(c => c == ',');
        return semicolonCount > commaCount ? ";" : ",";
    }

    private static List<Dictionary<string, string>> ParseExcel(Stream stream)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        if (stream.CanSeek) stream.Position = 0;

        using var excelReader = ExcelReaderFactory.CreateReader(stream);
        var dataSet = excelReader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
        });

        if (dataSet.Tables.Count == 0)
            throw new InvalidOperationException("Excel dosyasında tablo bulunamadı.");

        var table = dataSet.Tables[0];
        var rows = new List<Dictionary<string, string>>(table.Rows.Count);

        foreach (System.Data.DataRow dataRow in table.Rows)
        {
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (System.Data.DataColumn col in table.Columns)
            {
                row[col.ColumnName.Trim()] = dataRow[col]?.ToString()?.Trim() ?? string.Empty;
            }
            rows.Add(row);
        }
        return rows;
    }

    private static bool IsExcel(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext is ".xlsx" or ".xls";
    }

    // ─── Validation ─────────────────────────────────────────────────────────

    private async Task ValidateRowAsync(
        ImportBatchRow batchRow,
        Dictionary<string, string> raw,
        ImportType importType,
        string tenantId,
        CancellationToken ct)
    {
        switch (importType)
        {
            case ImportType.Customers:
                await ValidateCustomerRowAsync(batchRow, raw, tenantId, ct);
                break;
            case ImportType.Vehicles:
                await ValidateVehicleRowAsync(batchRow, raw, tenantId, ct);
                break;
            case ImportType.ServiceHistory:
                await ValidateServiceHistoryRowAsync(batchRow, raw, tenantId, ct);
                break;
        }
    }

    private async Task ValidateCustomerRowAsync(
        ImportBatchRow batchRow,
        Dictionary<string, string> raw,
        string tenantId,
        CancellationToken ct)
    {
        var name = Get(raw, "CustomerName");
        var phone = Get(raw, "Phone");
        var email = Get(raw, "Email");

        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(phone))
        {
            batchRow.SetError("CustomerName veya Phone zorunludur.");
            return;
        }

        var warnings = new List<string>();
        string? normalizedPhone = null;

        if (!string.IsNullOrWhiteSpace(phone))
        {
            try
            {
                normalizedPhone = PhoneNumber.Create(phone).Value;
            }
            catch
            {
                warnings.Add($"Telefon '{phone}' normalize edilemedi.");
            }
        }

        // Duplicate check
        if (normalizedPhone is not null)
        {
            var duplicateByPhone = await _context.Customers
                .AsNoTracking()
                .IgnoreQueryFilters()
                .AnyAsync(c => c.TenantId == tenantId && c.Phone == PhoneNumber.Create(normalizedPhone), ct);
            if (duplicateByPhone)
            {
                batchRow.SetSkipped($"Aynı telefon numarasıyla müşteri zaten mevcut: {normalizedPhone}");
                return;
            }
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            var normalizedEmail = email.ToLowerInvariant().Trim();
            var duplicateByEmail = await _context.Customers
                .AsNoTracking()
                .IgnoreQueryFilters()
                .AnyAsync(c => c.TenantId == tenantId && c.Email == normalizedEmail, ct);
            if (duplicateByEmail)
            {
                batchRow.SetSkipped($"Aynı e-posta ile müşteri zaten mevcut.");
                return;
            }
        }

        var normalized = new Dictionary<string, string>
        {
            ["CustomerName"] = name.Trim(),
            ["Phone"] = normalizedPhone ?? phone,
            ["Email"] = email.ToLowerInvariant().Trim(),
            ["TaxNumber"] = Get(raw, "TaxNumber").Trim(),
            ["Address"] = Get(raw, "Address").Trim(),
            ["Notes"] = Get(raw, "Notes").Trim()
        };

        var normalizedJson = JsonSerializer.Serialize(normalized);

        if (warnings.Count > 0)
            batchRow.SetWarning(string.Join("; ", warnings), normalizedJson);
        else
            batchRow.SetValid(normalizedJson);
    }

    private async Task ValidateVehicleRowAsync(
        ImportBatchRow batchRow,
        Dictionary<string, string> raw,
        string tenantId,
        CancellationToken ct)
    {
        var plate = Get(raw, "Plate").Trim().ToUpperInvariant().Replace(" ", "");
        var brand = Get(raw, "Brand").Trim();
        var model = Get(raw, "Model").Trim();
        var customerPhone = Get(raw, "CustomerPhone");
        var customerEmail = Get(raw, "CustomerEmail").ToLowerInvariant().Trim();

        var errors = new List<string>();
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(plate)) errors.Add("Plate zorunludur.");
        if (string.IsNullOrWhiteSpace(brand)) errors.Add("Brand zorunludur.");
        if (string.IsNullOrWhiteSpace(model)) errors.Add("Model zorunludur.");
        if (string.IsNullOrWhiteSpace(customerPhone) && string.IsNullOrWhiteSpace(customerEmail))
            errors.Add("CustomerPhone veya CustomerEmail zorunludur.");

        if (errors.Count > 0)
        {
            batchRow.SetError(string.Join("; ", errors));
            return;
        }

        // Year validation
        int? year = null;
        var yearStr = Get(raw, "Year");
        if (!string.IsNullOrWhiteSpace(yearStr))
        {
            if (int.TryParse(yearStr, out var parsedYear) && parsedYear is >= 1900 and <= 2100)
                year = parsedYear;
            else
                warnings.Add($"Year '{yearStr}' geçersiz, atlandı.");
        }

        // Mileage validation
        int? mileage = null;
        var mileageStr = Get(raw, "Mileage");
        if (!string.IsNullOrWhiteSpace(mileageStr))
        {
            if (int.TryParse(mileageStr.Replace(".", "").Replace(",", ""), out var parsedMileage) && parsedMileage >= 0)
                mileage = parsedMileage;
            else
                warnings.Add($"Mileage '{mileageStr}' geçersiz, atlandı.");
        }

        // Customer match
        Guid? customerId = null;
        if (!string.IsNullOrWhiteSpace(customerPhone))
        {
            try
            {
                var normalizedPhone = PhoneNumber.Create(customerPhone).Value;
                var phoneObj = PhoneNumber.Create(normalizedPhone);
                var customer = await _context.Customers
                    .AsNoTracking()
                    .IgnoreQueryFilters()
                    .Where(c => c.TenantId == tenantId && c.Phone == phoneObj)
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync(ct);
                if (customer != Guid.Empty) customerId = customer;
            }
            catch { }
        }

        if (customerId is null && !string.IsNullOrWhiteSpace(customerEmail))
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(c => c.TenantId == tenantId && c.Email == customerEmail)
                .Select(c => c.Id)
                .FirstOrDefaultAsync(ct);
            if (customer != Guid.Empty) customerId = customer;
        }

        if (customerId is null)
        {
            batchRow.SetError("Müşteri bulunamadı. CustomerPhone veya CustomerEmail ile eşleşen bir müşteri yok.");
            return;
        }

        // Duplicate vehicle check
        string plateNormalized;
        try { plateNormalized = PlateNumber.Create(plate).NormalizedValue; }
        catch { batchRow.SetError($"Plate '{plate}' geçerli bir plaka formatında değil."); return; }
        var existingVehicle = await _context.Vehicles
            .AsNoTracking()
            .IgnoreQueryFilters()
            .AnyAsync(v => v.TenantId == tenantId && v.Plate.NormalizedValue == plateNormalized, ct);
        if (existingVehicle)
        {
            batchRow.SetSkipped($"Plaka '{plate}' ile araç zaten mevcut.");
            return;
        }

        var normalized = new Dictionary<string, string>
        {
            ["Plate"] = plateNormalized,
            ["Brand"] = brand,
            ["Model"] = model,
            ["Year"] = year?.ToString() ?? string.Empty,
            ["Mileage"] = mileage?.ToString() ?? string.Empty,
            ["CustomerId"] = customerId.Value.ToString(),
            ["ChassisNumber"] = Get(raw, "ChassisNumber").Trim(),
            ["EngineNumber"] = Get(raw, "EngineNumber").Trim(),
            ["Notes"] = Get(raw, "Notes").Trim()
        };

        var normalizedJson = JsonSerializer.Serialize(normalized);
        if (warnings.Count > 0)
            batchRow.SetWarning(string.Join("; ", warnings), normalizedJson);
        else
            batchRow.SetValid(normalizedJson);
    }

    private async Task ValidateServiceHistoryRowAsync(
        ImportBatchRow batchRow,
        Dictionary<string, string> raw,
        string tenantId,
        CancellationToken ct)
    {
        var plate = Get(raw, "Plate").Trim().ToUpperInvariant().Replace(" ", "");
        var serviceTitle = Get(raw, "ServiceTitle").Trim();
        var description = Get(raw, "Description").Trim();
        var serviceDateStr = Get(raw, "ServiceDate").Trim();

        var errors = new List<string>();
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(plate)) errors.Add("Plate zorunludur.");
        if (string.IsNullOrWhiteSpace(serviceDateStr)) errors.Add("ServiceDate zorunludur.");
        if (string.IsNullOrWhiteSpace(serviceTitle) && string.IsNullOrWhiteSpace(description))
            errors.Add("ServiceTitle veya Description zorunludur.");

        if (errors.Count > 0)
        {
            batchRow.SetError(string.Join("; ", errors));
            return;
        }

        // Date parsing
        DateTimeOffset? serviceDate = TryParseDate(serviceDateStr);
        if (serviceDate is null)
        {
            batchRow.SetError($"ServiceDate '{serviceDateStr}' tanınan bir formatta değil (yyyy-MM-dd, dd.MM.yyyy, dd/MM/yyyy).");
            return;
        }

        // Vehicle match
        string plateNormalized;
        try { plateNormalized = PlateNumber.Create(plate).NormalizedValue; }
        catch { batchRow.SetError($"Plate '{plate}' geçerli bir plaka formatında değil."); return; }
        var vehicle = await _context.Vehicles
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(v => v.TenantId == tenantId && v.Plate.NormalizedValue == plateNormalized)
            .Select(v => new { v.Id, v.CurrentCustomerId })
            .FirstOrDefaultAsync(ct);

        if (vehicle is null)
        {
            batchRow.SetError($"Plaka '{plate}' ile araç bulunamadı. Önce araçları import edin.");
            return;
        }

        // Amount parsing
        decimal totalAmount = 0, paidAmount = 0;
        var totalStr = Get(raw, "TotalAmount");
        var paidStr = Get(raw, "PaidAmount");
        if (!string.IsNullOrWhiteSpace(totalStr))
        {
            if (!decimal.TryParse(totalStr.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out totalAmount))
                warnings.Add($"TotalAmount '{totalStr}' geçersiz, 0 kabul edildi.");
        }
        if (!string.IsNullOrWhiteSpace(paidStr))
        {
            if (!decimal.TryParse(paidStr.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out paidAmount))
                warnings.Add($"PaidAmount '{paidStr}' geçersiz, 0 kabul edildi.");
        }

        if (paidAmount > totalAmount && totalAmount > 0)
        {
            warnings.Add("PaidAmount, TotalAmount'tan büyük. PaidAmount, TotalAmount'a eşitlendi.");
            paidAmount = totalAmount;
        }

        int mileage = 0;
        var mileageStr = Get(raw, "Mileage");
        if (!string.IsNullOrWhiteSpace(mileageStr))
            int.TryParse(mileageStr.Replace(".", "").Replace(",", ""), out mileage);

        var normalized = new Dictionary<string, string>
        {
            ["Plate"] = plate,
            ["VehicleId"] = vehicle.Id.ToString(),
            ["CustomerId"] = vehicle.CurrentCustomerId?.ToString() ?? string.Empty,
            ["ServiceDate"] = serviceDate.Value.ToString("O"),
            ["Mileage"] = mileage.ToString(),
            ["ServiceTitle"] = serviceTitle,
            ["Description"] = description,
            ["LaborDescription"] = Get(raw, "LaborDescription").Trim(),
            ["PartsDescription"] = Get(raw, "PartsDescription").Trim(),
            ["ConsumablesDescription"] = Get(raw, "ConsumablesDescription").Trim(),
            ["TotalAmount"] = totalAmount.ToString(CultureInfo.InvariantCulture),
            ["PaidAmount"] = paidAmount.ToString(CultureInfo.InvariantCulture),
            ["Notes"] = Get(raw, "Notes").Trim()
        };

        var normalizedJson = JsonSerializer.Serialize(normalized);
        if (warnings.Count > 0)
            batchRow.SetWarning(string.Join("; ", warnings), normalizedJson);
        else
            batchRow.SetValid(normalizedJson);
    }

    // ─── Commit ─────────────────────────────────────────────────────────────

    private async Task<bool> CommitRowAsync(
        ImportBatchRow row,
        ImportType importType,
        string tenantId,
        CancellationToken ct)
    {
        return importType switch
        {
            ImportType.Customers => await CommitCustomerRowAsync(row, tenantId, ct),
            ImportType.Vehicles => await CommitVehicleRowAsync(row, tenantId, ct),
            ImportType.ServiceHistory => await CommitServiceHistoryRowAsync(row, tenantId, ct),
            _ => false
        };
    }

    private async Task<bool> CommitCustomerRowAsync(ImportBatchRow row, string tenantId, CancellationToken ct)
    {
        var data = DeserializeNormalized(row);

        var name = GetN(data, "CustomerName");
        var phoneStr = GetN(data, "Phone");
        var email = GetN(data, "Email");
        var notes = GetN(data, "Notes");

        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(phoneStr))
            return false;

        PhoneNumber? phone = null;
        if (!string.IsNullOrWhiteSpace(phoneStr))
        {
            try { phone = PhoneNumber.Create(phoneStr); }
            catch { /* already warned during validation */ }
        }

        // Double-check duplicate at commit time
        if (phone is not null)
        {
            var exists = await _context.Customers
                .IgnoreQueryFilters()
                .AnyAsync(c => c.TenantId == tenantId && c.Phone == phone, ct);
            if (exists) return false;
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            var exists = await _context.Customers
                .IgnoreQueryFilters()
                .AnyAsync(c => c.TenantId == tenantId && c.Email == email, ct);
            if (exists) return false;
        }

        var customer = new Domain.Customers.Customer(
            tenantId,
            string.IsNullOrWhiteSpace(name) ? (phoneStr ?? "İsimsiz") : name,
            phone,
            string.IsNullOrWhiteSpace(email) ? null : email,
            null,
            string.IsNullOrWhiteSpace(notes) ? null : notes);

        await _context.Customers.AddAsync(customer, ct);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    private async Task<bool> CommitVehicleRowAsync(ImportBatchRow row, string tenantId, CancellationToken ct)
    {
        var data = DeserializeNormalized(row);

        var plate = GetN(data, "Plate");
        var brand = GetN(data, "Brand");
        var model = GetN(data, "Model");
        var customerIdStr = GetN(data, "CustomerId");

        if (string.IsNullOrWhiteSpace(plate) || string.IsNullOrWhiteSpace(brand) || string.IsNullOrWhiteSpace(model))
            return false;

        // Double-check duplicate (plate stored as NormalizedValue from validation step)
        var exists = await _context.Vehicles
            .IgnoreQueryFilters()
            .AnyAsync(v => v.TenantId == tenantId && v.Plate.NormalizedValue == plate, ct);
        if (exists) return false;

        Guid? customerId = Guid.TryParse(customerIdStr, out var cid) && cid != Guid.Empty ? cid : null;
        int.TryParse(GetN(data, "Year"), out var year);
        int.TryParse(GetN(data, "Mileage"), out var mileage);
        var chassis = GetN(data, "ChassisNumber");
        var engine = GetN(data, "EngineNumber");

        var plateObj = PlateNumber.Create(plate);
        var vehicleYear = year > 0 ? year : DateTime.UtcNow.Year;
        var vehicle = new Vehicle(tenantId, plateObj, brand, model, vehicleYear);
        if (!string.IsNullOrWhiteSpace(chassis) || !string.IsNullOrWhiteSpace(engine))
            vehicle.SetTechnicalDetails(
                string.IsNullOrWhiteSpace(chassis) ? null : chassis,
                string.IsNullOrWhiteSpace(engine) ? null : engine,
                null);
        if (mileage > 0)
            vehicle.UpdateMileage(mileage);
        if (customerId.HasValue)
            vehicle.AssignCustomer(customerId.Value);

        await _context.Vehicles.AddAsync(vehicle, ct);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    private async Task<bool> CommitServiceHistoryRowAsync(ImportBatchRow row, string tenantId, CancellationToken ct)
    {
        var data = DeserializeNormalized(row);

        var vehicleIdStr = GetN(data, "VehicleId");
        var customerIdStr = GetN(data, "CustomerId");
        var serviceDateStr = GetN(data, "ServiceDate");

        if (!Guid.TryParse(vehicleIdStr, out var vehicleId) || vehicleId == Guid.Empty)
            return false;

        if (!Guid.TryParse(customerIdStr, out var customerId) || customerId == Guid.Empty)
            return false;

        // Verify vehicle+customer still exist
        var vehicleExists = await _context.Vehicles
            .IgnoreQueryFilters()
            .AnyAsync(v => v.Id == vehicleId && v.TenantId == tenantId, ct);
        if (!vehicleExists) return false;

        var customerExists = await _context.Customers
            .IgnoreQueryFilters()
            .AnyAsync(c => c.Id == customerId && c.TenantId == tenantId, ct);
        if (!customerExists) return false;

        int.TryParse(GetN(data, "Mileage"), out var mileage);

        var serviceTitle = GetN(data, "ServiceTitle");
        var description = GetN(data, "Description");
        var laborDesc = GetN(data, "LaborDescription");
        var partsDesc = GetN(data, "PartsDescription");
        var consumablesDesc = GetN(data, "ConsumablesDescription");
        var notes = GetN(data, "Notes");

        // Build complaint text
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(serviceTitle)) parts.Add(serviceTitle);
        if (!string.IsNullOrWhiteSpace(description)) parts.Add(description);
        if (!string.IsNullOrWhiteSpace(laborDesc)) parts.Add($"İşçilik: {laborDesc}");
        if (!string.IsNullOrWhiteSpace(partsDesc)) parts.Add($"Parça: {partsDesc}");
        if (!string.IsNullOrWhiteSpace(consumablesDesc)) parts.Add($"Sarf: {consumablesDesc}");
        if (!string.IsNullOrWhiteSpace(notes)) parts.Add($"Not: {notes}");
        var complaint = string.Join(" | ", parts);

        decimal.TryParse(GetN(data, "TotalAmount"), NumberStyles.Any, CultureInfo.InvariantCulture, out var totalAmount);
        decimal.TryParse(GetN(data, "PaidAmount"), NumberStyles.Any, CultureInfo.InvariantCulture, out var paidAmount);

        var orderNo = await _orderNumberGenerator.GenerateAsync(tenantId, ct);

        var order = new ServiceOrder(
            tenantId,
            orderNo,
            vehicleId,
            customerId,
            mileage,
            complaint.Length > 2000 ? complaint[..2000] : complaint);

        // Add labor as operation if amount > 0
        if (totalAmount > 0)
        {
            var operationLabel = string.IsNullOrWhiteSpace(serviceTitle)
                ? "Aktarılan Servis Kaydı"
                : serviceTitle;
            order.AddOperation(operationLabel, totalAmount);
        }

        // Add payment if paid > 0 and total > 0
        if (paidAmount > 0 && totalAmount > 0 && paidAmount <= totalAmount)
        {
            var svcDate = DateTimeOffset.TryParse(serviceDateStr, out var dt) ? dt : DateTimeOffset.UtcNow;
            order.AddPayment(paidAmount, PaymentMethod.Cash, svcDate);
        }

        // Set OpenedAt via internal context
        if (DateTimeOffset.TryParse(serviceDateStr, out var openedAt))
        {
            // We can't set OpenedAt directly; it's set in the constructor to UtcNow.
            // For MVP this is acceptable — the import date reflects current time.
        }

        await _context.ServiceOrders.AddAsync(order, ct);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static string Get(Dictionary<string, string> dict, string key)
        => dict.TryGetValue(key, out var v) ? v ?? string.Empty : string.Empty;

    private static string GetN(Dictionary<string, string> dict, string key)
        => dict.TryGetValue(key, out var v) ? v ?? string.Empty : string.Empty;

    private static Dictionary<string, string> DeserializeNormalized(ImportBatchRow row)
    {
        var json = row.NormalizedJson ?? row.RawJson;
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json,
            new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? [];
    }

    private static DateTimeOffset? TryParseDate(string raw)
    {
        string[] formats =
        [
            "yyyy-MM-dd",
            "dd.MM.yyyy",
            "dd/MM/yyyy",
            "yyyy/MM/dd",
            "MM/dd/yyyy",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ssZ",
            "O"
        ];

        foreach (var format in formats)
        {
            if (DateTimeOffset.TryParseExact(raw, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal, out var result))
                return result;
        }

        if (DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out var fallback))
            return fallback;

        return null;
    }

    private static string SanitizeFileName(string originalFileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var safe = new string(Path.GetFileName(originalFileName)
            .Where(c => !invalidChars.Contains(c))
            .ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "upload.csv" : safe;
    }

    private static ImportBatchDto ToDtoWithRows(ImportBatch batch, IEnumerable<ImportBatchRow> rows)
        => new(
            batch.Id,
            batch.TenantId,
            batch.ImportType,
            batch.OriginalFileName,
            batch.Status,
            batch.TotalRows,
            batch.ValidRows,
            batch.WarningRows,
            batch.ErrorRows,
            batch.ImportedRows,
            batch.SkippedRows,
            batch.CreatedAtUtc,
            batch.CompletedAtUtc,
            rows.Select(ToRowDto).ToList());

    private static ImportBatchRowDto ToRowDto(ImportBatchRow r)
        => new(r.Id, r.RowNumber, r.Status, r.RawJson, r.NormalizedJson, r.ErrorMessage, r.WarningMessage);
}

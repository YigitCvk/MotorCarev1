using System.Text;
using MotorCare.Application.Common.Interfaces;
using MotorCare.Application.ServiceOrders;
using MotorCare.Domain.Common;

namespace MotorCare.Api.Files;

public sealed class LocalServiceOrderAttachmentStorage : IServiceOrderAttachmentStorage
{
    private readonly IWebHostEnvironment _environment;

    public LocalServiceOrderAttachmentStorage(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<StoredAttachmentFile> SaveAsync(
        string tenantId,
        Guid serviceOrderId,
        IUploadedFile file,
        CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0)
        {
            throw new DomainException("Dosya boş olamaz.");
        }

        if (file.Length > ServiceOrderAttachmentFileRules.MaxFileSizeBytes)
        {
            throw new DomainException("Dosya boyutu 5 MB sınırını aşamaz.");
        }

        var originalFileName = Path.GetFileName(file.FileName).Trim();
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw new DomainException("Dosya adı geçersiz.");
        }

        if (originalFileName.Length > ServiceOrderAttachmentFileRules.MaxFileNameLength)
        {
            throw new DomainException("Dosya adı çok uzun.");
        }

        if (!ServiceOrderAttachmentFileRules.IsAllowedExtensionContentTypePair(originalFileName, file.ContentType))
        {
            throw new DomainException("Dosya uzantısı ve içerik tipi uyumsuz veya desteklenmiyor.");
        }

        var extension = ServiceOrderAttachmentFileRules.GetExtension(originalFileName);
        var safeFileName = BuildSafeFileName(originalFileName, extension);
        var now = DateTimeOffset.UtcNow;
        var relativeDirectory = Path.Combine(
            "uploads",
            "service-orders",
            SanitizePathSegment(tenantId),
            serviceOrderId.ToString("D"),
            now.Year.ToString("0000"),
            now.Month.ToString("00"));
        var relativePath = Path.Combine(relativeDirectory, safeFileName);
        var physicalPath = ResolvePhysicalPath(relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);

        await using var input = file.OpenReadStream();
        var header = new byte[12];
        var headerLength = await input.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);

        if (!HasExpectedSignature(header.AsSpan(0, headerLength), extension, file.ContentType))
        {
            throw new DomainException("Dosya içeriği izin verilen türle uyumlu değil.");
        }

        await using var output = File.Create(physicalPath);
        await output.WriteAsync(header.AsMemory(0, headerLength), cancellationToken);
        await input.CopyToAsync(output, cancellationToken);

        return new StoredAttachmentFile(
            safeFileName,
            originalFileName,
            ToStoragePath(relativePath),
            file.ContentType,
            file.Length);
    }

    public Task<Stream> OpenReadAsync(Domain.ServiceOrders.ServiceOrderAttachment attachment, CancellationToken cancellationToken = default)
    {
        var physicalPath = ResolvePhysicalPath(attachment.FilePath);
        if (!File.Exists(physicalPath))
        {
            throw new FileNotFoundException("Attachment file was not found.", physicalPath);
        }

        return Task.FromResult<Stream>(File.OpenRead(physicalPath));
    }

    public string GetDownloadUrl(Guid serviceOrderId, Guid attachmentId)
        => $"/api/service-orders/{serviceOrderId:D}/attachments/{attachmentId:D}/download";

    private string ResolvePhysicalPath(string relativePath)
    {
        if (Path.IsPathRooted(relativePath))
        {
            throw new DomainException("Dosya yolu geçersiz.");
        }

        var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;

        Directory.CreateDirectory(webRoot);

        var root = Path.GetFullPath(webRoot);
        var combined = Path.GetFullPath(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar)
            ? root
            : root + Path.DirectorySeparatorChar;

        if (!combined.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("Dosya yolu geçersiz.");
        }

        return combined;
    }

    private static string BuildSafeFileName(string originalFileName, string extension)
    {
        var stem = Path.GetFileNameWithoutExtension(originalFileName);
        var safeStem = SanitizeFileNameStem(stem);
        if (safeStem.Length > 80)
        {
            safeStem = safeStem[..80];
        }

        return $"{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}_{safeStem}{extension}";
    }

    private static string SanitizePathSegment(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            if (char.IsLetterOrDigit(c) || c is '-' or '_')
            {
                builder.Append(c);
            }
        }

        return builder.Length == 0 ? "tenant" : builder.ToString();
    }

    private static string SanitizeFileNameStem(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            if (char.IsLetterOrDigit(c) || c is '-' or '_')
            {
                builder.Append(c);
            }
            else if (char.IsWhiteSpace(c) || c is '.')
            {
                builder.Append('-');
            }
        }

        return builder.Length == 0 ? "file" : builder.ToString();
    }

    private static string ToStoragePath(string path)
        => path.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');

    private static bool HasExpectedSignature(ReadOnlySpan<byte> header, string extension, string contentType)
    {
        if (string.Equals(contentType, "image/jpeg", StringComparison.OrdinalIgnoreCase) &&
            (extension is ".jpg" or ".jpeg"))
        {
            return header.Length >= 3 &&
                   header[0] == 0xFF &&
                   header[1] == 0xD8 &&
                   header[2] == 0xFF;
        }

        if (string.Equals(contentType, "image/png", StringComparison.OrdinalIgnoreCase) && extension == ".png")
        {
            return header.Length >= 8 &&
                   header[0] == 0x89 &&
                   header[1] == 0x50 &&
                   header[2] == 0x4E &&
                   header[3] == 0x47 &&
                   header[4] == 0x0D &&
                   header[5] == 0x0A &&
                   header[6] == 0x1A &&
                   header[7] == 0x0A;
        }

        if (string.Equals(contentType, "image/webp", StringComparison.OrdinalIgnoreCase) && extension == ".webp")
        {
            return header.Length >= 12 &&
                   header[0] == 0x52 &&
                   header[1] == 0x49 &&
                   header[2] == 0x46 &&
                   header[3] == 0x46 &&
                   header[8] == 0x57 &&
                   header[9] == 0x45 &&
                   header[10] == 0x42 &&
                   header[11] == 0x50;
        }

        if (string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase) && extension == ".pdf")
        {
            return header.Length >= 4 &&
                   header[0] == 0x25 &&
                   header[1] == 0x50 &&
                   header[2] == 0x44 &&
                   header[3] == 0x46;
        }

        return false;
    }
}

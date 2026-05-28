using MotorCare.Application.Common.Interfaces;

namespace MotorCare.Api.Files;

public sealed class FormFileUpload : IUploadedFile
{
    private readonly IFormFile _file;

    public FormFileUpload(IFormFile file)
    {
        _file = file;
    }

    public string FileName => _file.FileName;

    public string ContentType => _file.ContentType;

    public long Length => _file.Length;

    public Stream OpenReadStream() => _file.OpenReadStream();
}

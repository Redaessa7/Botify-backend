namespace Botify.Infrastructure.CrossCutting.Services.FileServices;

public class FileValidator : IFileValidator
{
    private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB
    private readonly HashSet<string> _allowedExtensions = new() { ".pdf", ".jpg", ".png" };
    private readonly HashSet<string> _allowedContentTypes = new() { "application/pdf", "image/jpeg", "image/png" };

    public bool Validate(string fileName, string contentType, long fileSize)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();

        if (!_allowedExtensions.Contains(ext))
            return false;

        if (!_allowedContentTypes.Contains(contentType))
            return false;

        if (fileSize > _maxFileSize)
            return false;

        return true;
    }
}
namespace Botify.Infrastructure.Services.FileServices;

public interface IFileValidator
{
    bool Validate(string fileName, string contentType, long fileSize);
}
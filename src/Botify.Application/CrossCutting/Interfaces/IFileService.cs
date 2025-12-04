using Botify.Application.CrossCutting;
using Botify.Domain.ResultPattern;

namespace Botify.Application.CrossCutting.Interfaces;

public interface IFileService
{
    Task<Result<string>> SaveFileAsync(Stream fileStream, string fileName, string folder);
    Task<Stream?> GetFileAsync(string relativePath);
    Task DeleteFileAsync(string relativePath);
    Task<string> ReplaceFileAsync(Stream fileStream, string fileName, string oldRelativePath, string folder);
    
    Task<Result<FileDownloadResult>> DownloadFileAsync(string relativePath);
    

    public bool ValidateFile(string fileName, string contentType, long fileSize);
}
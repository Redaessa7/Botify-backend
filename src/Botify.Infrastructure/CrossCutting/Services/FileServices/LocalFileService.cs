using Botify.Domain.ResultPattern;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Botify.Application.CrossCutting;
using Botify.Application.CrossCutting.Interfaces;

namespace Botify.Infrastructure.CrossCutting.Services.FileServices;

public class LocalFileService : IFileService
{
    private readonly string _rootPath;
    private readonly IFileValidator _validator;
    private readonly ILogger<LocalFileService> _logger;

    public LocalFileService(string rootPath, IFileValidator validator, ILogger<LocalFileService> logger)
    {
        _rootPath = rootPath;
        _validator = validator;
        _logger = logger;
    }

    public bool ValidateFile(string fileName, string contentType, long fileSize)
        => _validator.Validate(fileName, contentType, fileSize);

    private string GetFullPath(string relativePath)
        => Path.Combine(_rootPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

    private string GenerateRelativePath(string folder, string fileName)
    {
        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        return Path.Combine(folder, uniqueFileName).Replace("\\", "/");
    }

    public async Task<Result<string>> SaveFileAsync(Stream fileStream, string fileName, string folder)
    {
        try
        {
            var relativePath = GenerateRelativePath(folder, fileName);
            var fullPath = GetFullPath(relativePath);

            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            await using var output = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(output);

            _logger.LogInformation("File saved successfully: {Path}", relativePath);
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FileName}", fileName);
            return Error.Unexpected("An error occurred while saving the file.");
        }
    }

    public async Task<Stream?> GetFileAsync(string relativePath)
    {
        var fullPath = GetFullPath(relativePath);
        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("File not found: {Path}", relativePath);
            return null;
        }

        _logger.LogInformation("File retrieved: {Path}", relativePath);
        return await Task.FromResult(new FileStream(fullPath, FileMode.Open, FileAccess.Read));
    }

    public Task DeleteFileAsync(string relativePath)
    {
        var fullPath = GetFullPath(relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("File deleted: {Path}", relativePath);
        }
        else
        {
            _logger.LogWarning("Attempted to delete non-existent file: {Path}", relativePath);
        }

        return Task.CompletedTask;
    }

    public async Task<Result<FileDownloadResult>> DownloadFileAsync(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || relativePath.Contains(".."))
        {
            _logger.LogWarning("Invalid file path requested: {Path}", relativePath);
            return Error.Conflict("Invalid file path.");
        }

        var fullPath = GetFullPath(relativePath);
        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("File not found for download: {Path}", relativePath);
            return Error.NotFound("File not found.");
        }

        var fileName = Path.GetFileName(fullPath);
        var contentTypeProvider = new FileExtensionContentTypeProvider();
        var contentType = contentTypeProvider.TryGetContentType(fileName, out var type)
            ? type
            : "application/octet-stream";

        try
        {
            var stream = await GetFileAsync(relativePath);
            if (stream is null)
            {
                _logger.LogError("File stream could not be opened: {Path}", relativePath);
                return Error.NotFound("File stream could not be opened.");
            }

            _logger.LogInformation("File download prepared: {Path}", relativePath);
            var result = new FileDownloadResult(stream, fileName, contentType);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file: {Path}", relativePath);
            return Error.Unexpected($"Failed to download file: {ex.Message}");
        }
    }

    public async Task<string> ReplaceFileAsync(Stream fileStream, string fileName, string oldRelativePath, string folder)
    {
        await DeleteFileAsync(oldRelativePath);
        var result = await SaveFileAsync(fileStream, fileName, folder);
        return result.Value;
    }
}

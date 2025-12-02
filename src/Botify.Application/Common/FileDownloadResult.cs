namespace Botify.Application.Common;

public class FileDownloadResult
{
    /// <summary>
    /// Stream representing the file content.
    /// </summary>
    public Stream FileStream { get; init; }

    /// <summary>
    /// Name of the file to be returned to the client.
    /// </summary>
    public string FileName { get; init; }

    /// <summary>
    /// MIME type of the file (e.g. application/pdf, image/png).
    /// </summary>
    public string ContentType { get; init; }

    public FileDownloadResult(Stream fileStream, string fileName, string contentType)
    {
        FileStream = fileStream;
        FileName = fileName;
        ContentType = contentType;
    }
}

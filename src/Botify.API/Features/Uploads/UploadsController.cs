using Botify.API.CrossCutting.Controllers;
using Botify.Application.CrossCutting.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Botify.API.Features.Uploads;

[Route("api/uploads")]
[ApiVersion("1.0")]
public class UploadsController(IFileService fileService) : ApiController
{
    private readonly IFileService _fileService = fileService;

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });

        // Validate file
        if (!_fileService.ValidateFile(file.FileName, file.ContentType, file.Length))
            return BadRequest(new { message = "Invalid file." });

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _fileService.SaveFileAsync(stream, file.FileName);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Errors });

            // Return the path with uploads prefix
            return Ok(new { filePath = $"https://localhost:7257/uploads/{result.Value}" , CreatedAt= DateTime.UtcNow});
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error.", details = ex.Message });
        }
    }
}
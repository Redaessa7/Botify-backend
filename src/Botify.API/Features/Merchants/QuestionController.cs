using Botify.API.CrossCutting.Controllers;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Features.Merchants.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Botify.API.Features.Merchants;

[ApiVersion("1.0")]
[Route("api/merchants/{merchantId:guid}/questions")]
public class QuestionController(IQuestionService questionService) : ApiController
{
    private readonly IQuestionService _questionService = questionService;

    #region Question Lifecycle

    /// <summary>
    /// Deletes a question.
    /// </summary>
    [HttpDelete("{questionId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteAsync(Guid merchantId, Guid questionId, CancellationToken ct = default)
    {
        var result = await _questionService.DeleteAsync(merchantId, questionId, ct);
        return result.Match(_ => Ok(new { }), Problem);
    }

    #endregion

    #region Queries

    /// <summary>
    /// Retrieves a question by ID.
    /// </summary>
    [HttpGet("{questionId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetByIdAsync(Guid merchantId, Guid questionId, CancellationToken ct = default)
    {
        var result = await _questionService.GetByIdAsync(merchantId, questionId, ct);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Retrieves all questions with pagination and optional search.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetQuestionsAsync(Guid merchantId, [FromQuery]string? cursor, [FromQuery] int limit = 10, [FromQuery] string? search = null, CancellationToken ct = default)
    {
        var result = await _questionService.GetQuestionsAsync(merchantId, cursor, limit, search, ct);
        return result.Match(Ok, Problem);
    }
    #region Stats

    /// <summary>
    /// Retrieves question statistics (total, pending, customer service).
    /// </summary>
    [HttpGet("stats")]
    [Authorize]
    public async Task<IActionResult> GetQuestionStatsAsync(Guid merchantId, CancellationToken ct = default)
    {
        var result = await _questionService.GetStatsAsync(merchantId, ct);
        return result.Match(Ok,Problem);
    }

    #endregion

    #endregion

    #region Status

    /// <summary>
    /// Toggles question resolved/unresolved status.
    /// </summary>
    [HttpPatch("{questionId:guid}/toggle")]
    [Authorize]
    public async Task<IActionResult> ToggleStatusAsync(Guid merchantId, Guid questionId, CancellationToken ct = default)
    {
        var result = await _questionService.ToggleStatusAsync(merchantId, questionId, ct);
        return result.Match(_ => Ok(new { }), Problem);
    }

    #endregion
}

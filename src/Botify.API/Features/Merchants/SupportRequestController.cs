using Botify.API.CrossCutting.Controllers;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Features.Merchants.Requests;
using Botify.Application.Features.Merchants.Responses;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.SupportRequests.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Botify.API.Features.Merchants;

[ApiVersion("1.0")]
[Route("api/merchants/{merchantId:guid}/support-requests")]
public class SupportRequestController(ISupportRequestService service) : ApiController
{
    private readonly ISupportRequestService _service = service;

    #region Question Lifecycle

    /// <summary>
    /// Deletes a question.
    /// </summary>
    [HttpDelete("{supportRequestId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteAsync(Guid merchantId, Guid supportRequestId, CancellationToken ct = default)
    {
        var result = await _service.DeleteAsync(merchantId, supportRequestId, ct);
        return result.Match(_ => Ok(new { }), Problem);
    }

    #endregion

    #region Queries

    /// <summary>
    /// Retrieves a question by ID.
    /// </summary>
    [HttpGet("{supportRequestId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetByIdAsync(Guid merchantId, Guid supportRequestId, CancellationToken ct = default)
    {
        var result = await _service.GetByIdAsync(merchantId, supportRequestId, ct);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Retrieves all questions with pagination and optional search.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllAsync(Guid merchantId,
                                                       [FromQuery] string? cursor,
                                                       [FromQuery] int limit = 10,
                                                       [FromQuery] string? search = null,
                                                       [FromQuery] bool? isResolved = null,
                                                       [FromQuery] BotPlatform? botPlatform = null,
                                                       [FromQuery] RequestReason? reason = null,
                                                       [FromQuery] SupportRequestType? type = null,
                                                       CancellationToken ct = default)
    {


         var result = await _service.GetAllAsync(merchantId, cursor, limit, search, isResolved, botPlatform,reason, type, ct);
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
        var result = await _service.GetStatsAsync(merchantId, ct);
        return result.Match(Ok,Problem);
    }

    #endregion

    #endregion


    #region Status

    /// <summary>
    /// Toggles question resolved/unresolved status.
    /// </summary>
    [HttpPatch("{supportRequestId:guid}/toggle")]
    [Authorize]
    public async Task<IActionResult> ToggleStatusAsync(Guid merchantId, Guid supportRequestId, CancellationToken ct = default)
    {
        var result = await _service.ToggleStatusAsync(merchantId, supportRequestId, ct);
        return result.Match(_ => Ok(new { }), Problem);
    }

    #endregion
}

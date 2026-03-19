using Botify.Application.CrossCutting.Helpers;
using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.CrossCutting.Options;
using Botify.Application.CrossCutting.Responses;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Features.Merchants.Requests;
using Botify.Application.Features.Merchants.Responses;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.SupportRequests;
using Botify.Domain.Models.SupportRequests.Enums;
using Botify.Domain.ResultPattern;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Botify.Application.Features.Merchants.Services;

public sealed class SupportRequestService : ISupportRequestService
{
    private readonly IApplicationDbContext _db;
    private readonly PaginationOptions _paginationOptions;
    private readonly ILogger<SupportRequestService> _logger;

    public SupportRequestService(IApplicationDbContext db, IOptions<PaginationOptions> options, ILogger<SupportRequestService> logger)
    {
        _db = db;
        _paginationOptions = options.Value;
        _logger = logger;
    }

    #region Question Lifecycle

    public async Task<Result<Success>> DeleteAsync(Guid merchantId, Guid questionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting support request QuestionId={QuestionId} MerchantId={MerchantId}", questionId, merchantId);

        var question = await _db.SupportRequests.FirstOrDefaultAsync(q => q.MerchantId == merchantId && q.Id == questionId, cancellationToken);
        if (question is null)
        {
            _logger.LogWarning("Support request not found QuestionId={QuestionId} MerchantId={MerchantId}", questionId, merchantId);
            return SupportRequestErrors.NotFound;
        }

        _db.SupportRequests.Remove(question);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Support request deleted successfully QuestionId={QuestionId} MerchantId={MerchantId}", questionId, merchantId);
        return Result.Success;
    }

    #endregion

    #region Queries

    public async Task<Result<SupportRequestDto>> GetByIdAsync(Guid merchantId, Guid questionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching support request QuestionId={QuestionId} MerchantId={MerchantId}", questionId, merchantId);

        var question = await _db.SupportRequests.FirstOrDefaultAsync(q => q.MerchantId == merchantId && q.Id == questionId, cancellationToken);
        if (question is null)
        {
            _logger.LogWarning("Support request not found QuestionId={QuestionId} MerchantId={MerchantId}", questionId, merchantId);
            return SupportRequestErrors.NotFound;
        }

        _logger.LogInformation("Support request retrieved successfully QuestionId={QuestionId} MerchantId={MerchantId}", questionId, merchantId);
        return SupportRequestDto.FromDomain(question);
    }

    public async Task<Result<PaginatedResult<SupportRequestDto>>> GetAllAsync(
        Guid merchantId,
        string? cursor,
        int limit = 10,
        string? search = null,
        bool? isResolve = null,
        BotPlatform? botPlatform = null,
        RequestReason? reason = null,
        SupportRequestType? type = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching support requests for MerchantId={MerchantId} Limit={Limit} Cursor={Cursor} Search={Search}", merchantId, limit, cursor, search);

        if (limit <= 0) limit = _paginationOptions.DefaultLimit;
        limit = Math.Min(limit, _paginationOptions.MaxLimit);

        CursorHelper.CursorPayload? payload = null;
        if (!string.IsNullOrEmpty(cursor))
        {
            payload = CursorHelper.Decode(cursor);
            if (payload == null)
            {
                _logger.LogWarning("Invalid cursor provided for MerchantId={MerchantId}", merchantId);
                return Error.Validation("Invalid cursor");
            }
        }

        var query = _db.SupportRequests.AsNoTracking().Where(q => q.MerchantId == merchantId);

        if (isResolve.HasValue) query = query.Where(q => q.IsResolved == isResolve.Value);
        if (botPlatform.HasValue) query = query.Where(q => q.Platform == botPlatform.Value);
        if (reason.HasValue) query = query.Where(q => q.Reason == reason.Value);
        if (type.HasValue) query = query.Where(q => q.Type == type.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(q => q.Customer.Name.ToLower().Contains(s) || q.Message.ToLower().Contains(s));
        }

        query = query.OrderByDescending(q => q.CreatedAtUtc).ThenByDescending(q => q.Id);

        var totalCount = await query.CountAsync(cancellationToken);

        if (payload != null)
        {
            query = query.Where(q =>
                q.CreatedAtUtc < payload.CreatedAt ||
                (q.CreatedAtUtc == payload.CreatedAt && q.Id.CompareTo(payload.Id) < 0));
        }


        // fetch one extra to detect next page
        var items = await query
            .Take(limit + 1)
            .Select(q => SupportRequestDto.FromDomain(q))
            .ToListAsync(cancellationToken);
            
        var hasMore = items.Count > limit;
        if (hasMore) items.RemoveAt(items.Count - 1);

        string? nextCursor = null;
        if (hasMore && items.Any())
        {
            var last = items.Last();
            var nextPayload = new CursorHelper.CursorPayload(last.Id, last.CreatedAt);
            nextCursor = CursorHelper.Encode(nextPayload);
        }

        _logger.LogInformation("Support requests retrieved for MerchantId={MerchantId}. Count={Count} Total={TotalCount} HasMore={HasMore}", merchantId, items.Count, totalCount, hasMore);

        var result = new PaginatedResult<SupportRequestDto>(items, limit, totalCount, nextCursor);
        return result;
    }

    public async Task<Result<SupportRequestStatsResponse>> GetStatsAsync(Guid merchantId, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching support request stats for MerchantId={MerchantId}", merchantId);

        var total = await _db.SupportRequests.AsNoTracking().CountAsync(q => q.MerchantId == merchantId, ct);
        var pending = await _db.SupportRequests.AsNoTracking().CountAsync(q => q.MerchantId == merchantId && !q.IsResolved, ct);
        var clientRequests = await _db.SupportRequests.AsNoTracking().CountAsync(q => q.MerchantId == merchantId && q.Reason == RequestReason.CLIENT_REQUEST, ct);
        var aiEscalated = await _db.SupportRequests.AsNoTracking().CountAsync(q => q.MerchantId == merchantId && q.Reason == RequestReason.AI_ESCALATED, ct);

        var stats = new SupportRequestStatsResponse
        {
            Total = total,
            Pending = pending,
            ClientRequests = clientRequests,
            AIEscalated = aiEscalated
        };

        _logger.LogInformation("Support request stats retrieved for MerchantId={MerchantId}. Total={Total}, Pending={Pending}, ClientRequests={ClientRequests}, AIEscalated={AIEscalated}",
            merchantId, total, pending, clientRequests, aiEscalated);

        return stats;
    }

    #endregion

    #region Status Management

    public async Task<Result<Success>> ToggleStatusAsync(Guid merchantId, Guid questionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Toggling support request status QuestionId={QuestionId} MerchantId={MerchantId}", questionId, merchantId);

        var question = await _db.SupportRequests.FirstOrDefaultAsync(q => q.MerchantId == merchantId && q.Id == questionId, cancellationToken);
        if (question is null)
        {
            _logger.LogWarning("Support request not found QuestionId={QuestionId} MerchantId={MerchantId}", questionId, merchantId);
            return SupportRequestErrors.NotFound;
        }

        var result = question.IsResolved ? question.Unresolve() : question.Resolve();
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to toggle support request status QuestionId={QuestionId} MerchantId={MerchantId}. Errors={Errors}", questionId, merchantId, result.Errors);
            return result;
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Support request status toggled successfully QuestionId={QuestionId} MerchantId={MerchantId} NewStatus={Status}", questionId, merchantId, question.IsResolved);

        return result;
    }

    #endregion
}

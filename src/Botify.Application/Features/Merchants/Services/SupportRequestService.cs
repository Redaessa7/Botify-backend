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

namespace Botify.Application.Features.Merchants.Services;

public sealed class SupportRequestService : ISupportRequestService
{
    private readonly IApplicationDbContext _db;
    private readonly PaginationOptions _paginationOptions;

    public SupportRequestService(IApplicationDbContext db, IOptions<PaginationOptions> options)
    {
        _db = db;
        _paginationOptions = options.Value;
    }

    #region Question Lifecycle


    public async Task<Result<Success>> DeleteAsync(
        Guid merchantId,
        Guid questionId,
        CancellationToken cancellationToken = default)
    {
        var question = await _db.SupportRequests
            .FirstOrDefaultAsync(q => q.MerchantId == merchantId && q.Id == questionId, cancellationToken);
        if (question is null) return SupportRequestErrors.NotFound;

        _db.SupportRequests.Remove(question);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }

    #endregion

    #region Queries

    public async Task<Result<SupportRequestDto>> GetByIdAsync(
        Guid merchantId,
        Guid questionId,
        CancellationToken cancellationToken = default)
    {
        var question = await _db.SupportRequests
            .FirstOrDefaultAsync(q => q.MerchantId == merchantId && q.Id == questionId, cancellationToken);
        if (question is null) return SupportRequestErrors.NotFound;

        return SupportRequestDto.FromDomain(question);
    }
    //public async Task<Result<PaginatedResult<SupportRequestDto>>> GetAllAsync(
    //Guid merchantId,
    //string? cursor,
    //int limit = 10,
    //string? search = null,
    //bool? isResolve = null,
    //BotPlatform? botPlatform = null,
    //CancellationToken cancellationToken = default)
    //{
    //    // --- FAKE DATA FOR TESTING ---
    //    var fakeItems = new List<SupportRequestDto>
    //{
    //    new SupportRequestDto
    //    {
    //        Id = Guid.NewGuid(),
    //        MerchantId = merchantId,
    //        BotId = Guid.NewGuid(),
    //        BotName = "SalesBot",
    //        Platform = BotPlatform.TELEGRAM,
    //        Type = SupportRequestType.GENERAL,
    //        Reason = RequestReason.CLIENT_REQUEST,
    //        ContactInfo = new ContactInfoResponse
    //        {
    //            CustomerName = "Mohammed Saif",
    //            PlatformContact = "@mohammed_saif"
    //        },
    //        Message = "I need help with my subscription.",
    //        IsResolved = false,
    //        CreatedAt = DateTime.UtcNow,
    //        ResolvedAt = null
    //    },
    //    new SupportRequestDto
    //    {
    //        Id = Guid.NewGuid(),
    //        MerchantId = merchantId,
    //        BotId = Guid.NewGuid(),
    //        BotName = "SupportBot",
    //        Platform = BotPlatform.WHATSAPP,
    //        Type = SupportRequestType.COMPLAINT,
    //        Reason = RequestReason.AI_ESCALATED,
    //        ContactInfo = new ContactInfoResponse
    //        {
    //            CustomerName = "Ali Ahmed",
    //            PlatformContact = "+967777654321"
    //        },
    //        Message = "The bot gave me the wrong answer.",
    //        IsResolved = true,
    //        CreatedAt = DateTime.UtcNow.AddDays(-2),
    //        ResolvedAt = DateTime.UtcNow.AddDays(-1)
    //    },
    //    new SupportRequestDto
    //    {
    //        Id = Guid.NewGuid(),
    //        MerchantId = merchantId,
    //        BotId = Guid.NewGuid(),
    //        BotName = "FeedbackBot",
    //        Platform = BotPlatform.MESSENGER,
    //        Type = SupportRequestType.FEEDBACK,
    //        Reason = RequestReason.CLIENT_REQUEST,
    //        ContactInfo = new ContactInfoResponse
    //        {
    //            CustomerName = "Sara Ali",
    //            PlatformContact = "sara.ali.fb"
    //        },
    //        Message = "Great experience, but could be faster.",
    //        IsResolved = false,
    //        CreatedAt = DateTime.UtcNow.AddHours(-5),
    //        ResolvedAt = null
    //    }
    //};

    //    var result = new PaginatedResult<SupportRequestDto>(
    //        fakeItems,
    //        limit,
    //        fakeItems.Count,
    //        nextCursor: null
    //    );

    //    return result;

    //    // --- END FAKE DATA ---

    //    // If you want to switch back to DB query later, just comment out the fake block above
    //    // and re-enable the EF Core query logic you already have.
    //}

    /// <summary>
    /// Get all SupportRequests with cursor-based pagination and optional search query.
    /// </summary>
    public async Task<Result<PaginatedResult<SupportRequestDto>>> GetAllAsync(
        Guid merchantId,
        string? cursor,
        int limit = 10,
        string? search = null,
            bool? isResolve = null,
        BotPlatform? botPlatform = null,
        CancellationToken cancellationToken = default)
    {
        // enforce limits
        if (limit <= 0) limit = _paginationOptions.DefaultLimit;
        limit = Math.Min(limit, _paginationOptions.MaxLimit);

        // decode cursor if provided
        CursorHelper.CursorPayload? payload = null;
        if (!string.IsNullOrEmpty(cursor))
        {
            payload = CursorHelper.Decode(cursor);
            if (payload == null)
                return Error.Validation("Invalid cursor");
        }

        // base query
        var query = _db.SupportRequests.AsNoTracking()
            .Where(q => q.MerchantId == merchantId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(q =>
                q.Customer.Name.Contains(s) ||
                q.Message.Contains(s));
        }

        if (isResolve.HasValue)
        {
            query = query.Where(q => q.IsResolved == isResolve.Value);
        }

        if (botPlatform.HasValue)
        {
            query = query.Where(q => q.Platform == botPlatform.Value);
        }

        // order newest-first
        query = query.OrderByDescending(q => q.CreatedAtUtc)
                     .ThenByDescending(q => q.Id);

        // total count for metadata
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
        if (hasMore)
            items.RemoveAt(items.Count - 1);

        // build next cursor
        string? nextCursor = null;
        if (hasMore && items.Any())
        {
            var last = items.Last();
            var nextPayload = new CursorHelper.CursorPayload(last.Id, last.CreatedAt);
            nextCursor = CursorHelper.Encode(nextPayload);
        }

        var result = new PaginatedResult<SupportRequestDto>(items, limit, totalCount, nextCursor);
        return result;
    }


    public async Task<Result<SupportRequestStatsResponse>> GetStatsAsync(Guid merchantId, CancellationToken ct = default)
    {
        var total = await _db.SupportRequests
            .CountAsync(q => q.MerchantId == merchantId, ct);

        var pending = await _db.SupportRequests
            .CountAsync(q => q.MerchantId == merchantId && !q.IsResolved, ct);

        var stats = new SupportRequestStatsResponse
        {
            Total = total,
            Pending = pending
        };

        return stats;
    }
    #endregion

    #region Status Management

    /// <summary>
    /// Toggle question resolved/unresolved status.
    /// </summary>
    public async Task<Result<Success>> ToggleStatusAsync(
        Guid merchantId,
        Guid questionId,
        CancellationToken cancellationToken = default)
    {
        var question = await _db.SupportRequests
            .FirstOrDefaultAsync(q => q.MerchantId == merchantId && q.Id == questionId, cancellationToken);
        if (question is null) return SupportRequestErrors.NotFound;

        var result = question.IsResolved ? question.Unresolve() : question.Resolve();
        if (!result.IsSuccess) return result;

        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    #endregion
}

using Botify.Application.CrossCutting.Helpers;
using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.CrossCutting.Options;
using Botify.Application.CrossCutting.Responses;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Features.Merchants.Requests;
using Botify.Application.Features.Merchants.Responses;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.Questions;
using Botify.Domain.ResultPattern;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Botify.Application.Features.Merchants.Services;

public sealed class QuestionService : IQuestionService
{
    private readonly IApplicationDbContext _db;
    private readonly PaginationOptions _paginationOptions;

    public QuestionService(IApplicationDbContext db, IOptions<PaginationOptions> options)
    {
        _db = db;
        _paginationOptions = options.Value;
    }

    #region Question Lifecycle

    public async Task<Result<QuestionResponse>> CreateAsync(
        Guid merchantId,
        CreateQuestionRequest request,
        CancellationToken cancellationToken = default)
    {
        var merchant = await _db.Merchants
            .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);
        if (merchant is null) return MerchantErrors.NotFound;

        var questionResult = request.ToQuestion(merchantId);
        if (!questionResult.IsSuccess)
            return questionResult.Errors;

        var question = questionResult.Value;
        _db.Questions.Add(question);

        await _db.SaveChangesAsync(cancellationToken);
        return QuestionResponse.FromDomain(question);
    }

    public async Task<Result<Success>> DeleteAsync(
        Guid merchantId,
        Guid questionId,
        CancellationToken cancellationToken = default)
    {
        var question = await _db.Questions
            .FirstOrDefaultAsync(q => q.MerchantId == merchantId && q.Id == questionId, cancellationToken);
        if (question is null) return QuestionErrors.NotFound;

        _db.Questions.Remove(question);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }

    #endregion

    #region Queries

    public async Task<Result<QuestionResponse>> GetByIdAsync(
        Guid merchantId,
        Guid questionId,
        CancellationToken cancellationToken = default)
    {
        var question = await _db.Questions
            .FirstOrDefaultAsync(q => q.MerchantId == merchantId && q.Id == questionId, cancellationToken);
        if (question is null) return QuestionErrors.NotFound;

        return QuestionResponse.FromDomain(question);
    }

    /// <summary>
    /// Get all questions with cursor-based pagination and optional search query.
    /// </summary>
    public async Task<Result<PaginatedResult<QuestionResponse>>> GetQuestionsAsync(
        Guid merchantId,
        string? cursor,
        int limit = 10,
        string? search = null,
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
        var query = _db.Questions.AsNoTracking()
            .Where(q => q.MerchantId == merchantId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(q =>
                q.Customer.Name.Contains(s) ||
                q.Message.Contains(s));
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
            .Select(q => QuestionResponse.FromDomain(q))
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

        var result = new PaginatedResult<QuestionResponse>(items, limit, totalCount, nextCursor);
        return result;
    }


    public async Task<Result<QuestionStatsResponse>> GetStatsAsync(Guid merchantId, CancellationToken ct = default)
    {
        var total = await _db.Questions
            .CountAsync(q => q.MerchantId == merchantId, ct);

        var pending = await _db.Questions
            .CountAsync(q => q.MerchantId == merchantId && !q.IsResolved, ct);

        var stats = new QuestionStatsResponse
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
        var question = await _db.Questions
            .FirstOrDefaultAsync(q => q.MerchantId == merchantId && q.Id == questionId, cancellationToken);
        if (question is null) return QuestionErrors.NotFound;

        var result = question.IsResolved ? question.Unresolve() : question.Resolve();
        if (!result.IsSuccess) return result;

        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    #endregion
}

using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.CrossCutting.Responses;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Features.Merchants.Requests;
using Botify.Application.Features.Merchants.Responses;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.Questions;
using Botify.Domain.ResultPattern;
using Microsoft.EntityFrameworkCore;

namespace Botify.Application.Features.Merchants.Services;

public sealed class QuestionService : IQuestionService
{
    private readonly IApplicationDbContext _db;

    public QuestionService(IApplicationDbContext db)
    {
        _db = db;
    }

    #region Question Lifecycle

    public async Task<Result<QuestionResponse>> CreateAsync(Guid merchantId, CreateQuestionRequest request, CancellationToken cancellationToken = default)
    {
        var merchant = await _db.Merchants.FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);
        if (merchant is null) return MerchantErrors.NotFound;

        var questionResult = request.ToQuestion(merchantId);

        if (!questionResult.IsSuccess)
            return questionResult.Errors;

        var question = questionResult.Value;

        _db.Questions.Add(question);
        await _db.SaveChangesAsync(cancellationToken);

        return QuestionResponse.FromDomain(question);
    }

    public async Task<Result<Success>> DeleteAsync(Guid merchantId, Guid questionId, CancellationToken cancellationToken = default)
    {
        var question = await _db.Questions.FirstOrDefaultAsync(q => q.MerchantId == merchantId && q.Id == questionId, cancellationToken);
        if (question is null) return QuestionErrors.NotFound;

        _db.Questions.Remove(question);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }

    #endregion

    #region Queries

    public async Task<Result<QuestionResponse>> GetByIdAsync(Guid merchantId, Guid questionId, CancellationToken cancellationToken = default)
    {
        var question = await _db.Questions.FirstOrDefaultAsync(q => q.MerchantId == merchantId && q.Id == questionId, cancellationToken);
        if (question is null) return QuestionErrors.NotFound;

        return QuestionResponse.FromDomain(question);
    }

    /// <summary>
    /// Get all questions with pagination and optional search query.
    /// </summary>
    public async Task<Result<PaginatedResult<QuestionResponse>>> GetQuestionsAsync(
        Guid merchantId,
        int page = 1,
        int limit = 10,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Questions.Where(q => q.MerchantId == merchantId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(q =>
                q.Customer.Name.Contains(search) ||   // ✅ updated to use CustomerInfo
                q.Message.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var responses = items.Select(QuestionResponse.FromDomain).ToList();

        var paginated = new PaginatedResult<QuestionResponse>(responses, page, limit, totalCount);
        return paginated;
    }

    #endregion

    #region Status Management

    /// <summary>
    /// Toggle question resolved/unresolved status.
    /// </summary>
    public async Task<Result<Success>> ToggleStatusAsync(Guid merchantId, Guid questionId, CancellationToken cancellationToken = default)
    {
        var question = await _db.Questions.FirstOrDefaultAsync(q => q.MerchantId == merchantId && q.Id == questionId, cancellationToken);
        if (question is null) return QuestionErrors.NotFound;

        var result = question.IsResolved ? question.Unresolve() : question.Resolve();
        if (!result.IsSuccess) return result;

        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    #endregion
}

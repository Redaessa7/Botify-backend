using Botify.Application.CrossCutting.Helpers;
using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.CrossCutting.Options;
using Botify.Application.CrossCutting.Responses;
using Botify.Application.Features.AI.Interfaces;
using Botify.Application.Features.AI.Requests;
using Botify.Application.Features.AI.Responses;
using Botify.Domain.Models.AI;
using Botify.Domain.Models.AI.Enums;
using Botify.Domain.Models.Merchants;
using Botify.Domain.ResultPattern;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Botify.Application.Features.AI.Services;

public sealed class AIConfigurationService : IAIConfigurationService
{
    private readonly IApplicationDbContext _db;
    private readonly PaginationOptions _paginationOptions;

    public AIConfigurationService(IApplicationDbContext db, IOptions<PaginationOptions> options)
    {
        _db = db;
        _paginationOptions = options.Value;
    }

    #region AIConfiguration Lifecycle

    public async Task<Result<AIConfigurationResponse>> CreateAsync(
        Guid merchantId,
        CreateAIConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        var merchant = await _db.Merchants
            .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);

        if (merchant is null)
            return MerchantErrors.NotFound;
        var platform = Enum.Parse<AIPlatform>(request.Platform, true);
        var configResult = AIConfiguration.Create(
            Guid.NewGuid(),
            request.SystemPrompt,
            request.ModelName,
            request.ApiKey,
            platform,
            request.Temperature,
            request.MaxTokens,
            merchantId);

        if (!configResult.IsSuccess)
            return configResult.Errors;

        var config = configResult.Value;
        _db.AIConfigurations.Add(config);

        await _db.SaveChangesAsync(cancellationToken);

        return AIConfigurationResponse.FromDomain(config);
    }

    public async Task<Result<Success>> UpdateAsync(
        Guid merchantId,
        Guid configId,
        UpdateAIConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        var config = await _db.AIConfigurations
            .FirstOrDefaultAsync(c => c.MerchantId == merchantId && c.Id == configId, cancellationToken);

        if (config is null)
            return AIConfigurationErrors.NotFound;
        var platform = Enum.Parse<AIPlatform>(request.Platform, true);
        // Apply updates
        config.Update(
            request.SystemPrompt,
            request.ModelName,
            request.ApiKey,
            platform,
            request.Temperature,
            request.MaxTokens);

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success;
    }
    #endregion

    #region Queries

    public async Task<Result<AIConfigurationResponse>> GetByIdAsync(
        Guid merchantId,
        Guid configId,
        CancellationToken cancellationToken = default)
    {
        var config = await _db.AIConfigurations
            .FirstOrDefaultAsync(c => c.MerchantId == merchantId && c.Id == configId, cancellationToken);

        if (config is null)
            return AIConfigurationErrors.NotFound;

        return AIConfigurationResponse.FromDomain(config);
    }

    public async Task<Result<PaginatedResult<AIConfigurationResponse>>> GetConfigurationsAsync(
        Guid merchantId,
        string? cursor,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (limit <= 0) limit = _paginationOptions.DefaultLimit;
        limit = Math.Min(limit, _paginationOptions.MaxLimit);

        CursorHelper.CursorPayload? payload = null;
        if (!string.IsNullOrEmpty(cursor))
        {
            payload = CursorHelper.Decode(cursor);
            if (payload == null)
                return Error.Validation("Invalid cursor");
        }

        var query = _db.AIConfigurations.AsNoTracking()
            .Where(c => c.MerchantId == merchantId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .ThenByDescending(c => c.Id);

        var totalCount = await query.CountAsync(cancellationToken);

        if (payload != null)
        {
            query = query.Where(c =>
                c.CreatedAtUtc < payload.CreatedAt ||
                (c.CreatedAtUtc == payload.CreatedAt && c.Id.CompareTo(payload.Id) < 0))
                .OrderByDescending(x=> x.CreatedAtUtc);
        }

        var items = await query
            .Take(limit + 1)
            .Select(c => AIConfigurationResponse.FromDomain(c))
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > limit;
        if (hasMore)
            items.RemoveAt(items.Count - 1);

        string? nextCursor = null;
        if (hasMore && items.Any())
        {
            var last = items.Last();
            var nextPayload = new CursorHelper.CursorPayload(last.Id, last.CreateAt);
            nextCursor = CursorHelper.Encode(nextPayload);
        }

        var result = new PaginatedResult<AIConfigurationResponse>(items, limit, totalCount, nextCursor);
        return result;
    }

    #endregion
}

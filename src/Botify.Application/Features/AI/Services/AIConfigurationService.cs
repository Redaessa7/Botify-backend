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

    public async Task<Result<AIConfigurationResponse>> UpdateAsync(
        Guid merchantId,
        SetAIConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        var merchant = await _db.Merchants.Include(x => x.AIConfiguration)
            .FirstOrDefaultAsync(c => c.Id == merchantId, cancellationToken);


        if (merchant is null)
            return MerchantErrors.NotFound;
        var platform = Enum.Parse<AIPlatform>(request.Platform, true);

        AIConfiguration? aIConfiguration = null;
        if (merchant.AIConfiguration is not null)
        {
                            var configResult = merchant.AIConfiguration.Update(
                request.SystemPrompt,
                request.ModelName,
                request.ApiKey,
                platform,
                request.Temperature,
                request.MaxTokens,
                request.OrderProcessingEnabled,
                request.ProductRecommendationsEnabled,
                request.SmallTalkEnabled,
                request.AutoEscalateEnabled
                );
           if(!configResult.IsSuccess)
                return configResult.Errors;
            aIConfiguration = merchant.AIConfiguration;

        }
        else
        {
            var configResult = AIConfiguration.Create(
                Guid.NewGuid(),
                request.SystemPrompt,
                request.ModelName,
                request.ApiKey,
                platform,
                request.Temperature,
                request.MaxTokens,
                merchantId,
                request.OrderProcessingEnabled,
                request.ProductRecommendationsEnabled,
                request.SmallTalkEnabled,
                request.AutoEscalateEnabled);
            if (!configResult.IsSuccess)
                return configResult.Errors;
            var config = configResult.Value;

            var merchatLogic = merchant.AddConfigureAI(config);

            if (!merchatLogic.IsSuccess)
                return merchatLogic.Errors;

            aIConfiguration = config;
            _db.AIConfigurations.Add(config);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return AIConfigurationResponse.FromDomain(aIConfiguration);
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

    public async Task<Result<AIConfigurationResponse>> GetConfigurationsAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default)
    {


        var aIConfiguration = _db.AIConfigurations.AsNoTracking()
            .Where(c => c.MerchantId == merchantId).FirstOrDefault();
        if (aIConfiguration == null)
        {
           return AIConfigurationErrors.NotFound;
        }

       
        return AIConfigurationResponse.FromDomain(aIConfiguration);
    }

    #endregion
}

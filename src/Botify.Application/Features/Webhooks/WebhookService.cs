using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.Features.AI.Interfaces;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Features.Webhooks.Requests;
using Botify.Domain.Models.AI.Enums;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.Enums;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.ResultPattern;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Botify.Application.Features.Webhooks;

public class WebhookService : IWebhookService
{
    private readonly IApplicationDbContext _context;
    private readonly IKernelExecutionService _kernelExecutionService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(
        IApplicationDbContext context,
        IKernelExecutionService kernelExecutionService,
        IServiceProvider serviceProvider,
        ILogger<WebhookService> logger)
    {
        _context = context;
        _kernelExecutionService = kernelExecutionService;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    #region Authentication

    public async Task<Result<bool>> AuthenticateAsync(Guid botId, string secretKey, CancellationToken ct)
    {
        _logger.LogInformation("Authenticating bot BotId={BotId}", botId);

        var exists = await _context.Bots.AnyAsync(b => b.Id == botId && b.Secret == secretKey, ct);

        if (!exists)
        {
            _logger.LogWarning("Authentication failed for BotId={BotId}", botId);
            return Error.Failure("Authentication failed");
        }

        _logger.LogInformation("Authentication succeeded for BotId={BotId}", botId);
        return true;
    }

    #endregion

    #region Webhook Handling

    public async Task<Result<string>> HandleAsync(TelegramRequest telegramRequest, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling webhook for BotId={BotId} ChatId={ChatId}", telegramRequest.BotId, telegramRequest.ChatId);

        var merchant = await _context.Merchants
            .Include(m => m.Bots)
            .Include(m => m.AIConfiguration)
            .Include(m => m.Subscription)
            .Where(m => m.Bots.Any(b => b.Id == telegramRequest.BotId && b.Secret == telegramRequest.SecretKey))
            .FirstOrDefaultAsync(cancellationToken);

        if (merchant?.AIConfiguration is null)
        {
            _logger.LogWarning("Merchant or AI configuration not found for BotId={BotId}", telegramRequest.BotId);
            return MerchantErrors.NotFound;
        }

        var pluginTier = merchant.Subscription.CurrentPlan switch
        {
            Plan.Free => PluginTier.Free,
            Plan.Pro => PluginTier.Pro,
            _ => throw new NotImplementedException()
        };

        var bot = merchant.Bots.First(b => b.Id == telegramRequest.BotId);
        var customer = _context.Customers.FirstOrDefault(c => c.MerchantId == merchant.Id && c.Telegram.ChatId == telegramRequest.ChatId.ToString());

        var botPlatformService = _serviceProvider.GetRequiredKeyedService<IBotPlatformService>(bot.Platform.AsKey());

        try
        {
            _logger.LogInformation("Executing AI model for BotId={BotId} MerchantId={MerchantId}", bot.Id, merchant.Id);

            var aiResult = await _kernelExecutionService.ExecuteAsync(
                new KernelExecutionRequest(
                    telegramRequest.UserMessage,
                    merchant.AIConfiguration,
                    pluginTier,
                    customer,
                    BotPlatform.TELEGRAM,
                    telegramRequest.ChatId.ToString()),
                cancellationToken);

            if (aiResult.IsError)
            {
                _logger.LogWarning("AI execution failed for BotId={BotId} MerchantId={MerchantId}. Errors={Errors}", bot.Id, merchant.Id, aiResult.Errors);
                return aiResult.Errors;
            }

            var sendResult = await botPlatformService.SendMessageAsync(
                bot.Token,
                telegramRequest.ChatId,
                aiResult.Value,
                cancellationToken);

            if (sendResult.IsError)
            {
                _logger.LogError("Failed to send message to Telegram for BotId={BotId} MerchantId={MerchantId}. Errors={Errors}", bot.Id, merchant.Id, sendResult.Errors);
                return sendResult.Errors;
            }

            _logger.LogInformation("Webhook handled successfully BotId={BotId} MerchantId={MerchantId}", bot.Id, merchant.Id);
            return aiResult.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while handling webhook for BotId={BotId} MerchantId={MerchantId}", telegramRequest.BotId, merchant?.Id);
            return Error.Failure("Exception occurred while handling webhook");
        }
    }

    #endregion
}

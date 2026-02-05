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

namespace Botify.Application.Features.Webhooks;

public class WebhookService(IApplicationDbContext context, IKernelExecutionService kernelExecutionService, IServiceProvider serviceProvider) : IWebhookService
{
    private readonly IApplicationDbContext _context = context;
    private readonly IKernelExecutionService _kernelExecutionService = kernelExecutionService;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    public async Task<Result<bool>> AuthenticateAsync(Guid botId, string secretKey, CancellationToken ct)
    {
        return await _context.Bots.AnyAsync(b => b.Id == botId && b.Secret == secretKey, ct);
    }

    public async Task<Result<string>> HandleAsync(TelegramRequest telegramRequest, CancellationToken cancellationToken)
    {
        var merchant = await _context.Merchants.Include(m=> m.Bots)
            .Include((m=>m.AIConfiguration))
            .Include((m=>m.Subscription))
            .Where(m=> m.Bots.Any(b=>b.Id == telegramRequest.BotId && b.Secret == telegramRequest.SecretKey))
            .FirstOrDefaultAsync(cancellationToken);
       
        if (merchant?.AIConfiguration is null)
            return MerchantErrors.NotFound;
        var pluginTier = merchant.Subscription.CurrentPlan switch
        {
            Plan.Free => PluginTier.Free,
            Plan.Pro => PluginTier.Pro,
            _ => throw new NotImplementedException(),
        };

        var bot = merchant.Bots.First(b => b.Id == telegramRequest.BotId);
        var customer = _context.Customers.FirstOrDefault(c => c.MerchantId == merchant.Id && c.Telegram.ChatId ==  telegramRequest.ChatId.ToString());
        
        var _botPlatformService = _serviceProvider.GetRequiredKeyedService<IBotPlatformService>(bot.Platform.AsKey());
        Console.WriteLine("Now Enter to Ai Model.");
        try
        {
            var aiResult = await _kernelExecutionService.ExecuteAsync(
             new KernelExecutionRequest(
                 telegramRequest.UserMessage,
                 merchant.AIConfiguration,
                 pluginTier,
                 customer),
              cancellationToken);

            if (aiResult.IsError)
                return aiResult.Errors;

            // 2️⃣ Send message to Telegram
            var sendResult = await _botPlatformService.SendMessageAsync(
                bot.Token,
                telegramRequest.ChatId,
                aiResult.Value,
                cancellationToken
            );

            if (sendResult.IsError)
                return sendResult.Errors;

            // 3️⃣ Success
            return aiResult.Value;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "";
        }
    }
}
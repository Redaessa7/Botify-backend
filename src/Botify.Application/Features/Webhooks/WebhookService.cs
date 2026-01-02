using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.Features.AI.Interfaces;
using Botify.Application.Features.Webhooks.Requests;
using Botify.Domain.Models.AI.Enums;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.Enums;
using Botify.Domain.ResultPattern;
using Microsoft.EntityFrameworkCore;

namespace Botify.Application.Features.Webhooks;

public class WebhookService(IApplicationDbContext context, IKernelExecutionService kernelExecutionService) : IWebhookService
{
    private readonly IApplicationDbContext _context = context;
    private readonly IKernelExecutionService _kernelExecutionService = kernelExecutionService;

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
            Plan.Enterprise => PluginTier.Enterprise,
            Plan.Pro => PluginTier.Premium,
            _ => throw new NotImplementedException(),
        };
        
        Console.WriteLine("Now Enter to Ai Model.");
        try
        {
           return await _kernelExecutionService.ExecuteAsync(telegramRequest.UserMessage, merchant.AIConfiguration,pluginTier,cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "";
        }
    }
}
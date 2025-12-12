using Botify.Application.CrossCutting.Interfaces;
using Botify.Domain.ResultPattern;
using Microsoft.EntityFrameworkCore;

namespace Botify.Application.Features.Webhooks;

public class WebhookService(IApplicationDbContext context) : IWebhookService
{
    private readonly IApplicationDbContext context = context;

    public async Task<Result<bool>> AuthenticateAsync(Guid botId, string secretKey, CancellationToken ct)
    {
        return await context.Bots.AnyAsync(b => b.Id == botId && b.Secret == secretKey, ct);
    }

    public async Task<Result<Success>> HandleAsync(string platform, Guid botId, object payload, CancellationToken cancellationToken)
    {
        return Result.Success;
    }
}
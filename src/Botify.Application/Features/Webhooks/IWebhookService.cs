using Botify.Application.Features.Webhooks.Requests;
using Botify.Domain.ResultPattern;

namespace Botify.Application.Features.Webhooks;

public interface IWebhookService
{
    Task<Result<bool>> AuthenticateAsync(Guid botId, string secretKey, CancellationToken ct);
    Task<Result<string>> HandleAsync(TelegramRequest telegramRequest, CancellationToken cancellationToken);

}
using Botify.Domain.ResultPattern;

namespace Botify.Application.Features.Webhooks;

public interface IWebhookService
{
    Task<Result<bool>> AuthenticateAsync(Guid botId, string secretKey, CancellationToken ct);
    Task<Result<Success>> HandleAsync(string platform, Guid botId, object payload, CancellationToken cancellationToken);

}
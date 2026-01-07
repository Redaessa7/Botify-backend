using Botify.Domain.ResultPattern;

namespace Botify.Application.Features.Merchants.Interfaces
{
    public interface IBotPlatformService
    {
        string PlatformName { get; }
        Task<Result<Success>> ValidateTokenAsync(string apiToken, CancellationToken ct = default);
            Task<Result<Success>> SendMessageAsync(
        string apiToken,
        long chatId,
        string message,
        CancellationToken ct = default);
        Task<Result<string>> SetWebhookAsync(string apiToken, string webhookUrl,string secretToken, CancellationToken ct = default);
    }

}

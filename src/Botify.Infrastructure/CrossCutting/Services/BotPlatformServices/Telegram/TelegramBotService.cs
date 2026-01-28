using Botify.Application.Features.Merchants.Interfaces;
using Botify.Domain.ResultPattern;
using Botify.Infrastructure.CrossCutting.Services.BotPlatformServices.Telegram.Models;
using System.Net.Http.Json;
using Botify.Domain.Models.Bots;

namespace Botify.Infrastructure.CrossCutting.Services.BotPlatformServices.Telegram;
public sealed class TelegramBotService : IBotPlatformService
{
    private readonly HttpClient _http;
    public TelegramBotService(HttpClient http) => _http = http;

    public string PlatformName => "telegram";

    public async Task<Result<Success>> ValidateTokenAsync(string apiToken, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync($"https://api.telegram.org/bot{apiToken}/getMe", ct);
        if (!resp.IsSuccessStatusCode) return BotErrors.TokenInvalid;

        var payload = await resp.Content.ReadFromJsonAsync<TelegramGetMeResponse>(cancellationToken: ct);
        if (payload?.Ok != true || payload.Result?.IsBot != true) return BotErrors.TokenInvalid;

        return Result.Success;
    }
    public async Task<Result<Success>> SendMessageAsync(
 string apiToken,
 long chatId,
 string message,
 CancellationToken ct = default)
    {
        var url = $"https://api.telegram.org/bot{apiToken}/sendMessage";

        var payload = new TelegramSendMessageRequest(chatId, message);

        var resp = await _http.PostAsJsonAsync(url, payload, ct);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            Console.WriteLine($"Telegram sendMessage failed: {body}");
            return BotErrors.SendMessageFailed;
        }

        var result = await resp.Content.ReadFromJsonAsync<TelegramSendMessageResponse>(cancellationToken: ct);

        return result?.Ok == true
            ? Result.Success
            : BotErrors.SendMessageFailed;
    }

    public async Task<Result<string>> SetWebhookAsync(string apiToken, string webhookUrl, string secretToken, CancellationToken ct = default)
    {
        var url = $"https://api.telegram.org/bot{apiToken}/setWebhook?url={Uri.EscapeDataString(webhookUrl)}&secret_token={Uri.EscapeDataString(secretToken)}";

        var resp = await _http.GetAsync(url, ct);
        if (!resp.IsSuccessStatusCode) return BotErrors.WebhookUrlInvalid;

        return webhookUrl;
    }

}

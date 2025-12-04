using Botify.Application.Features.Merchants.Interfaces;
using Botify.Domain.Models.Merchants.Bots;
using Botify.Domain.ResultPattern;
using Botify.Infrastructure.CrossCutting.Services.BotPlatformServices.Telegram.Models;
using System.Net.Http.Json;

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

    public async Task<Result<string>> SetWebhookAsync(string apiToken, string webhookUrl, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync($"https://api.telegram.org/bot{apiToken}/setWebhook?url={Uri.EscapeDataString(webhookUrl)}", ct);
        if (!resp.IsSuccessStatusCode) return BotErrors.WebhookUrlInvalid;

        return webhookUrl;
    }
}

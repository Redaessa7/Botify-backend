using Botify.API.CrossCutting.Controllers;
using Botify.Application.Features.Webhooks;
using Botify.Application.Features.Webhooks.Requests;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Botify.API.Features.Webhooks;

[Route("api/webhook/")]
[ApiVersion("1.0")]
public class WebhooksController(IWebhookService webhookService) : ApiController
{
    private readonly IWebhookService webhookService = webhookService;

        // /api/webhook/telegram/6eee19ac-554c-436d-a6c5-179b0ee8b25d
    // POST api/webhook/bot/{platform}/{botId}
    [HttpPost("{platform}/{botId}")]
    public async Task<IActionResult> ReceiveAsync(
        string platform,
        Guid botId,
        [FromBody] Update update)
    {
        Console.WriteLine($"Received update {update}");
        if (string.IsNullOrWhiteSpace(platform))
            return BadRequest("Platform is required.");

        if (!HttpContext.Request.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var headerVals))
            return Unauthorized();


        var request = new TelegramRequest(botId,headerVals!,update.Message.Chat.Id, update.Message.Text);
        var result = await webhookService.HandleAsync(request, CancellationToken.None);

        return result.Match(Ok, Problem);
    }
}

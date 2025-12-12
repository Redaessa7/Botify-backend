using Botify.API.CrossCutting.Controllers;
using Botify.Application.Features.Webhooks;
using Microsoft.AspNetCore.Mvc;

namespace Botify.API.Features.Webhooks;

[Route("api/webhook/bot")]
[ApiVersion("1.0")]
public class WebhooksController(IWebhookService webhookService) : ApiController
{
    private readonly IWebhookService webhookService = webhookService;

    // POST api/webhook/bot/{platform}/{botId}
    [HttpPost("{platform}/{botId}")]
    public async Task<IActionResult> ReceiveAsync(
        string platform,
        Guid botId,
        object payload)
    {
        if (string.IsNullOrWhiteSpace(platform))
            return BadRequest("Platform is required.");

        if (!HttpContext.Request.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var headerVals))
            return Unauthorized();

        var result = await webhookService.AuthenticateAsync(botId, headerVals!, default);

        if (!result.Value)
            return Unauthorized();

        //TODO: send payload to handler 

        await webhookService.HandleAsync(platform, botId, payload, default);
        
        Console.WriteLine(new
        {
            Platform = platform,
            BotId = botId,
            Time = DateTime.UtcNow
        });

        return Ok();
    }
}

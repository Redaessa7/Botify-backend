using Botify.API.CrossCutting.Controllers;
using Botify.Application.CrossCutting.Requests;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Features.Merchants.Requests;
using Botify.Application.Features.Merchants.Responses;
using Botify.Application.Services.Merchants;
using Botify.Application.Services.Merchants.Requests;
using Botify.Domain.Models.Bots;
using Botify.Domain.Models.Merchants.Enums;
using Botify.Domain.Models.Merchants.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Botify.API.Features.Merchants;

[ApiVersion("1.0")]
[Route("api/merchants")]
public class MerchantController(IMerchantService merchantService, IServiceProvider serviceProvider) : ApiController
{
    private readonly IMerchantService _merchantService = merchantService;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    // -------------------- GET --------------------

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentMerchant()
    {
        var result = await _merchantService.GetCurrentMerchantAsync();
        return result.Match(Ok, Problem);
    }
    [HttpGet("{merchantId:guid}/home")]
    [Authorize]
    public async Task<ActionResult<HomePageInitDataResponse>> GetHomePageAsync(Guid merchantId, CancellationToken ct)
    {
        var result = await _merchantService.GetHomePageAsync(merchantId,ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("{merchantId:guid}/bots")]
    [Authorize]
    public async Task<IActionResult> GetBotsAsync(Guid merchantId, CancellationToken ct)
    {
        var result = await _merchantService.GetBotsAsync(merchantId, ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("{merchantId:guid}/bots/{botId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetBotByIdAsync(Guid merchantId, Guid botId, CancellationToken ct)
    {
        var result = await _merchantService.GetBotByIdAsync(merchantId, botId, ct);
        return result.Match(Ok, Problem);
    }


    // -------------------- PATCH (Update) --------------------
    [HttpPatch("{merchantId:guid}/update-basic-info")]
    [Authorize]
    public async Task<IActionResult> UpdateBasicInfoAsync(Guid merchantId, UpdateMerchantBasicInfoRequest request, CancellationToken ct)
    {
        var result = await _merchantService.UpdateBasicInfoAsync(merchantId, request, ct);
        return result.Match(_ => Ok(new {}), Problem);
    }
    [HttpPatch("{merchantId:guid}/preferences")]
    [Authorize]
    public async Task<IActionResult> UpdatePreferencesAsync(Guid merchantId, [FromBody] int preferences, CancellationToken ct)
    {
        var result = await _merchantService.UpdatePreferencesAsync(merchantId, (QuickActions)preferences, ct);
        return result.Match(
            _ => Ok(new { merchantId, preferences }),
            errors => Problem(errors)
        );
    }

    [HttpPatch("{merchantId:guid}/bots/{botId:guid}/name")]
    [Authorize]
    public async Task<IActionResult> UpdateBotNameAsync(Guid merchantId, Guid botId, [FromBody] string newName, CancellationToken ct)
    {
        var result = await _merchantService.UpdateBotNameAsync(merchantId, botId, newName, ct);
        return result.Match(
            _ => Ok(new { merchantId, botId, name = newName }),
            errors => Problem(errors)
        );
    }

    [HttpPatch("{merchantId:guid}/bots/{botId:guid}/activate")]
    [Authorize]
    public async Task<IActionResult> ActivateBotAsync(Guid merchantId, Guid botId, CancellationToken ct)
    {
        var result = await _merchantService.ActivateBotAsync(merchantId, botId, ct);
        return result.Match(_ => Ok(new { }), errors => Problem(errors));
    }

    [HttpPatch("{merchantId:guid}/bots/{botId:guid}/deactivate")]
    [Authorize]
    public async Task<IActionResult> DeactivateBotAsync(Guid merchantId, Guid botId, CancellationToken ct)
    {
        var result = await _merchantService.DeactivateBotAsync(merchantId, botId, ct);
        return result.Match(_ => Ok(new { }), errors => Problem(errors));
    }

    // -------------------- POST (Create) --------------------

    [HttpPost("{merchantId:guid}/bots")]
    [Authorize]
    public async Task<IActionResult> CreateBotAsync(Guid merchantId, CreateBotRequest request)
    {
        var result = await _merchantService.CreateBotAsync(merchantId, request);
        return result.Match(Ok, Problem);
    }

    [HttpPost("bots/check-token")]
    [Authorize]
    public async Task<IActionResult> CheckValidToken(CheckTokenRequest request, CancellationToken ct)
    {
        var platformService = _serviceProvider.GetKeyedService<IBotPlatformService>(request.BotPlatform.AsKey());
        if (platformService is null)
            return Problem([BotErrors.PlatformInvalid]);

        var result = await platformService.ValidateTokenAsync(request.ApiToken, ct);
        return result.Match(_ => Ok(new { valid = true }), errors => Problem(errors));
    }

    // -------------------- DELETE --------------------

    [HttpDelete("{merchantId:guid}/bots/{botId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteBotAsync(Guid merchantId, Guid botId, CancellationToken ct)
    {
        var result = await _merchantService.DeleteBotAsync(merchantId, botId, ct);
        return result.Match(
            _ => Ok(new { message = "Bot deleted successfully." }),
            errors => Problem(errors)
        );
    }
}

using Botify.API.CrossCutting.Controllers;
using Botify.Application.CrossCutting.Requests;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Services.Auths;
using Botify.Application.Services.Merchants;
using Botify.Application.Services.Merchants.Responses;
using Botify.Domain.Models.Merchants.Bots;
using Botify.Domain.ResultPattern;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Botify.API.Features.Merchants;

[ApiVersion("1.0")]
[Route("api/merchants")]
public class MerchantController(IMerchantService merchantService, IServiceProvider serviceProvider) : ApiController
{
    private readonly IMerchantService _merchantService = merchantService;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentMerchant()
    {
        var result = await _merchantService.GetCurrentMerchantAsync();
        return result.Match(value => Ok(value), Problem);
    }

    [HttpPost("{merchantId:guid}/bots")]
    [Authorize]
    public async Task<IActionResult> CreateBotAsync(Guid merchantId, CreateBotRequest request)
    {
        var result = await _merchantService.CreateBotAsync(merchantId, request);
        return result.Match(value => Ok(value), Problem);
    }

    [HttpPost("bots/check-token")]
    [Authorize] 
    public async Task<IActionResult> CheckValidToken([FromBody] CheckTokenRequest request, CancellationToken ct)
    {
        // Resolve platform service via KeyedServices
        var platformService = _serviceProvider.GetKeyedService<IBotPlatformService>(request.Platform.ToUpperInvariant());
        if (platformService is null)
            return Problem([BotErrors.PlatformInvalid]);
        var result = await platformService.ValidateTokenAsync(request.ApiToken, ct);
        return result.Match(
            _ => Ok(new { valid = true }),
            errors => Problem(errors)
        );
    }

    [HttpGet("{merchantId:guid}/bots")]
    [Authorize]
    public async Task<IActionResult> GetBotsAsync(Guid merchantId, CancellationToken ct)
    {
        var result = await _merchantService.GetBotsAsync(merchantId, ct);

        return result.Match(
            bots => Ok(bots),
            errors => Problem(errors)
        );
    }

    [HttpPatch("{merchantId:guid}/bots/{botId:guid}/activate")]
    [Authorize]
    public async Task<IActionResult> ActivateBotAsync(Guid merchantId, Guid botId, CancellationToken ct)
    {
        var result = await _merchantService.ActivateBotAsync(merchantId, botId, ct);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors)
        );
    }

    [HttpPatch("{merchantId:guid}/bots/{botId:guid}/deactivate")]
    [Authorize]
    public async Task<IActionResult> DeactivateBotAsync(Guid merchantId, Guid botId, CancellationToken ct)
    {
        var result = await _merchantService.DeactivateBotAsync(merchantId, botId, ct);

        return result.Match(
            _ => NoContent(),
            errors => Problem(errors)
        );
    }
}

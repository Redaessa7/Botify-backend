using Botify.API.Extensions;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Requests;
using Botify.Application.Services.Auths;
using Botify.Application.Services.Merchants;
using Botify.Domain.Models.Merchants.Bots;
using Botify.Domain.ResultPattern;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Botify.API.Controllers;

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
    [AllowAnonymous] // or [Authorize] if you want to restrict
    public async Task<IActionResult> CheckValidToken([FromBody] CheckTokenRequest request, CancellationToken ct)
    {
        // Resolve platform service via KeyedServices
        var platformService = _serviceProvider.GetKeyedService<IBotPlatformService>(request.Platform.ToLowerInvariant());
        if (platformService is null)
            return Problem([BotErrors.PlatformInvalid]);
        var result = await platformService.ValidateTokenAsync(request.ApiToken, ct);
        return result.Match(
            _ => Ok(new { valid = true }),
            errors => Problem(errors)
        );
    }
}

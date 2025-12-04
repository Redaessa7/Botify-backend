using Botify.API.Extensions;
using Botify.Application.Requests;
using Botify.Application.Services.Auths;
using Botify.Application.Services.Merchants;
using Botify.Domain.ResultPattern;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using static Botify.Application.Services.Merchants.MerchantService;

namespace Botify.API.Controllers;

[Route("api/merchants")]
public class MerchantController(IMerchantService merchantService) : ApiController
{
   private readonly IMerchantService _merchantService = merchantService;

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentMerchant()
    {
        var result = await _merchantService.GetCurrentMerchantAsync();

        return result.Match(value => Ok(value), Problem);
    }


    [HttpPost("{merchantId:guid}/bots")]
    [Authorize]
    public async Task<IActionResult> CreateBotAsync(Guid merchantId,CreateBotRequest request)
    {
        var result = await _merchantService.AddBotAsync(merchantId,request);
        return result.Match(value => Ok(value), Problem);
    }
}

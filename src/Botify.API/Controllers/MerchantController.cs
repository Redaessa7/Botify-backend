using Botify.API.Extensions;
using Botify.Application.Requests;
using Botify.Application.Services.Auths;
using Botify.Application.Services.Merchants;
using Botify.Domain.ResultPattern;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        if (!result.IsSuccess)
            return Problem(result.Errors);
        return Ok(result.Value);
    }
}

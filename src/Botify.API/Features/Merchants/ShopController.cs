using Botify.API.CrossCutting.Controllers;
using Botify.Application.Features.Merchants.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Botify.API.Features.Merchants
{
    [Route("api/merchants/{merchantId:guid}/shops")]
    [Authorize]
    public class ShopController :  ApiController
    {
        private readonly IMerchantService _merchantService;

        public ShopController(IMerchantService merchantService)
        {
            _merchantService = merchantService;
        }

        // GET: api/merchants/{merchantId}/shops
        [HttpGet]
        public async Task<IActionResult> GetShopAsync(Guid merchantId)
        {
            var result = await _merchantService.GetShopAsync(merchantId, HttpContext.RequestAborted);
            return result.Match(Ok, Problem);
        }

        // GET: api/merchants/{merchantId}/shops/exists
        [HttpGet("exists")]
        public async Task<IActionResult> CheckShopExistsAsync(Guid merchantId)
        {
            var result = await _merchantService.CheckShopExistsAsync(merchantId, HttpContext.RequestAborted);
            return result.Match(result => Ok(new { exists = result}), Problem);
        }
    }

}

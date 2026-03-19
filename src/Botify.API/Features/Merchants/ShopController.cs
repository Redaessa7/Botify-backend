using Botify.API.CrossCutting.Controllers;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Features.Merchants.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Botify.API.Features.Merchants
{
    [Route("api/merchants/{merchantId:guid}/shops")]
    [Authorize]
    public class ShopController : ApiController
    {
        private readonly IMerchantService _merchantService;

        public ShopController(IMerchantService merchantService)
        {
            _merchantService = merchantService;
        }

        // -------------------- PUT --------------------
        [HttpPut]
        public async Task<IActionResult> SetShopAsync(Guid merchantId, SetShopRequest request, CancellationToken ct)
        {
            var result = await _merchantService.SetShopAsync(merchantId, request, ct);
            return result.Match(Ok, Problem);
        }

        // -------------------- GET --------------------
        [HttpGet]
        public async Task<IActionResult> GetShopAsync(Guid merchantId, CancellationToken ct)
        {
            var result = await _merchantService.GetShopAsync(merchantId, ct);
            return result.Match(Ok, Problem);
        }

        [HttpGet("exists")]
        public async Task<IActionResult> CheckShopExistsAsync(Guid merchantId, CancellationToken ct)
        {
            var result = await _merchantService.CheckShopExistsAsync(merchantId, ct);
            return result.Match(r => Ok(new { exists = r }), Problem);
        }

        // -------------------- DELETE --------------------
        [HttpDelete]
        public async Task<IActionResult> DeleteShopAsync(Guid merchantId, CancellationToken ct)
        {
            var result = await _merchantService.DeleteShopAsync(merchantId, ct);
            return result.Match(
                _ => Ok(new { message = "Shop deleted successfully." }),
                Problem
            );
        }
    }
}

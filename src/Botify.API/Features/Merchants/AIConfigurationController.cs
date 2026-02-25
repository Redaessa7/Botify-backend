using Botify.API.CrossCutting.Controllers;
using Botify.Application.Features.AI.Interfaces;
using Botify.Application.Features.AI.Requests;
using Botify.Application.Features.AI.Responses;
using Botify.Application.Features.AI.Strategies;
using Botify.Domain.Models.AI.Enums;
using Botify.Domain.ResultPattern;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Botify.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/merchants/{merchantId:guid}/ai-configurations")]
    public sealed class AIConfigurationController : ApiController
    {
        private readonly IAIConfigurationService _service;
        private readonly IServiceProvider _provider;

        public AIConfigurationController(IAIConfigurationService service, IServiceProvider provider)
        {
            _service = service;
            _provider = provider;
        }

        #region CRUD Endpoints

        [HttpPut("set")]
        [Authorize]
        public async Task<IActionResult> Set(
            Guid merchantId,
            [FromBody] SetAIConfigurationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.UpdateAsync(merchantId,request, cancellationToken);
            return result.Match(Ok, Problem);
        }

        #endregion

        #region Token Validation

        [HttpPost("/api/ai-validate-token")]
        public async Task<IActionResult> ValidateAIToken(TokenValidationRequest request)
        {

            var strategy = _provider.GetKeyedService<IAIPlatformStrategy>(request.Platform.AsKey());

            if (strategy is null)
                return Problem($"Platform {request.Platform} not supported");

            var result = await strategy.ValidateTokenAsync(request);

            return result.Match(
                _ => Ok(new { Valid = true })
                ,Problem);
        }

        #endregion

        #region Queries

        [HttpGet("{configId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetById(
            Guid merchantId,
            Guid configId,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(merchantId, configId, cancellationToken);
            return result.Match(Ok, Problem);
        }

        [HttpGet("exists")]
        public async Task<IActionResult> IsAIConfigExistsAsync(Guid merchantId, CancellationToken cancellationToken)
        {
            var result = await _service.IsAIConfigExistsAsync(merchantId, cancellationToken);
            return result.Match(result => Ok(new { exists = result }), Problem);
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetConfigurations(
            Guid merchantId,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetConfigurationsAsync(merchantId, cancellationToken);
            return result.Match(Ok, Problem);
        }

        #endregion
    }

 
}

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
    [Route("api/merchants/{merchantId:guid}/ai-configurations/")]
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
            if(!Enum.IsDefined(typeof(AIPlatform), request.Platform))
                return Problem($"Platform {request.Platform} not supported");

            var strategy = _provider.GetKeyedService<IAIPlatformStrategy>(request.Platform.ToUpperInvariant());

            if (strategy is null)
                return Problem($"Platform {request.Platform} not supported");

            var isValid = await strategy.ValidateTokenAsync(request.ApiKey, request.BaseUrl);
            Result<Success> result = isValid ? Result.Success : Error.Validation("TokenValidationFailed", "The provided API key could not be validated.");

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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetConfigurations(
            Guid merchantId,
            [FromQuery] string? cursor,
            [FromQuery] int limit = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetConfigurationsAsync(merchantId, cursor, limit, cancellationToken);
            return result.Match(Ok, Problem);
        }

        #endregion
    }

 
}

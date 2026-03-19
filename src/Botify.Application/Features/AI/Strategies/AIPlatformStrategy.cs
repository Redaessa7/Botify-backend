using Botify.Application.Features.AI.Requests;
using Botify.Domain.Models.AI.Enums;
using Botify.Domain.ResultPattern;
using Microsoft.Extensions.Logging;

namespace Botify.Application.Features.AI.Strategies
{
    public sealed class AIPlatformStrategy : IAIPlatformStrategy
    {
        private readonly IAIPlatformStrategy _strategy;
        private readonly ILogger<AIPlatformStrategy> _logger;

        public AIPlatformStrategy(IAIPlatformStrategy strategy, ILogger<AIPlatformStrategy> logger)
        {
            _strategy = strategy;
            _logger = logger;
        }

        public async Task<Result<Success>> ValidateTokenAsync(TokenValidationRequest request)
        {
            try
            {
                _logger.LogInformation("Validating token for Platform={Platform} Model={Model}", request.Platform, request.Model);
                return await _strategy.ValidateTokenAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while validating token for Platform={Platform} Model={Model}", request.Platform, request.Model);
                return Error.Failure("Token validation failed due to an internal error");
            }
        }
    }
}

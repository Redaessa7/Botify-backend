using Botify.Application.Features.AI.Requests;
using Botify.Domain.Models.AI.Enums;
using Botify.Domain.ResultPattern;

namespace Botify.Application.Features.AI.Strategies
{
    public sealed class AIPlatformStrategy(IAIPlatformStrategy strategy) : IAIPlatformStrategy
    {
        private readonly IAIPlatformStrategy _strategy = strategy;
        public async Task<Result<Success>> ValidateTokenAsync(TokenValidationRequest request)
        {
            return await _strategy.ValidateTokenAsync(request);
        }
    }
}

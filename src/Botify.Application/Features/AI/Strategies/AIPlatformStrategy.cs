using Botify.Domain.Models.AI.Enums;

namespace Botify.Application.Features.AI.Strategies
{
    public sealed class AIPlatformStrategy(IAIPlatformStrategy strategy) : IAIPlatformStrategy
    {
        private readonly IAIPlatformStrategy _strategy = strategy;
        public async Task<bool> ValidateTokenAsync(string apiKey,string? model = null, string? baseUrl = null)
        {
            return await _strategy.ValidateTokenAsync(apiKey, baseUrl);
        }
    }
}

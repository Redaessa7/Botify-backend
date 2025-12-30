using Botify.Application.Features.AI.Strategies;

namespace Botify.Infrastructure.AIServices.Strategies
{
    public sealed class GeminiStrategy : IAIPlatformStrategy
    {
        private readonly HttpClient _client;

        public GeminiStrategy(HttpClient client) => _client = client;

        public async Task<bool> ValidateTokenAsync(string apiKey, string? baseUrl = null)
        {
            var response = await _client.GetAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}");
            return response.IsSuccessStatusCode;
        }
    }
}

using System.Net.Http;
using System.Threading.Tasks;
using Botify.Application.Features.AI.Strategies;

namespace Botify.Infrastructure.AIServices.Strategies
{
    public sealed class OpenAIStrategy : IAIPlatformStrategy
    {
        private readonly HttpClient _client;

        public OpenAIStrategy(HttpClient client) => _client = client;

        public async Task<bool> ValidateTokenAsync(string apiKey, string? model = null, string? baseUrl = null)
        {
            // Default base URL for OpenAI if none provided
            var endpoint = baseUrl ?? "https://api.openai.com/v1/models";

            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}

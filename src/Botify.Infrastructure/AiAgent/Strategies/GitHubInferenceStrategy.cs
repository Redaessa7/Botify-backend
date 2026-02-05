using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Botify.Application.Features.AI.Strategies;

namespace Botify.Infrastructure.AIServices.Strategies
{
    public sealed class GitHubInferenceStrategy : IAIPlatformStrategy
    {
        private readonly HttpClient _client;

        public GitHubInferenceStrategy(HttpClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Validate the API token by making a lightweight request to the GitHub Models inference endpoint.
        /// </summary>
        public async Task<bool> ValidateTokenAsync(string apiKey, string? model = null, string? baseUrl = null)
        {
            try
            {
                // Default GitHub Models inference endpoint
                var endpoint = baseUrl ?? "https://models.github.ai/inference/chat/completions";

                // Build minimal request payload
                var payload = new
                {
                    model = model ?? "gpt-4o", // default model
                    messages = new[]
                    {
                        new { role = "system", content = "ping" }
                    },
                    max_tokens = 1
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Attach Bearer token
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                // GitHub requires a User-Agent header
                _client.DefaultRequestHeaders.UserAgent.ParseAdd("BotifyApp/1.0");

                var response = await _client.PostAsync(endpoint, content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}

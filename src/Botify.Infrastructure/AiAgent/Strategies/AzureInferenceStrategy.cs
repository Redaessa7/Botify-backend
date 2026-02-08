using Azure;
using Azure.AI.Inference;
using Botify.Application.Features.AI.Strategies;

namespace Botify.Infrastructure.AIServices.Strategies
{
    public sealed class AzureInferenceStrategy : IAIPlatformStrategy
    {
        private readonly ChatCompletionsClient _client;

        public AzureInferenceStrategy(ChatCompletionsClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Validate the API token by making a lightweight request to the inference endpoint.
        /// </summary>
        public async Task<bool> ValidateTokenAsync(string apiKey,string? model = null, string? baseUrl = null)
        {
            try
            {
                var endpoint = baseUrl != null ? new Uri(baseUrl) : new Uri("https://models.github.ai/inference");
                var credential = new AzureKeyCredential(apiKey);

                var client = new ChatCompletionsClient(endpoint, credential, new AzureAIInferenceClientOptions());

                var options = new ChatCompletionsOptions
                {
                    Model = "gpt-4o",
                    MaxTokens = 1,
                    Messages =
                    {
                        new ChatRequestSystemMessage("ping")
                    }
                };

                var response = await client.CompleteAsync(options);
                return response != null && response.GetRawResponse().Status == 200;
            }
            catch
            {
                return false;
            }
        }
    }
}

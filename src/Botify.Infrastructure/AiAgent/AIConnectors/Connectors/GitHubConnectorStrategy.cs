using Botify.Domain.Models.AI;
using Botify.Domain.Models.AI.Enums;
using Botify.Infrastructure.AIServices.AIConnectors.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Botify.Infrastructure.AIServices.AIConnectors.Connectors
{
    internal class GitHubConnectorStrategy : IAIConnectorStrategy
    {
        public AIPlatform PlatformKey => AIPlatform.GitHub;

        public async Task ConfigureBuilder(IKernelBuilder builder, AIConfiguration config)
        {
            var endpoint = new Uri("https://models.github.ai/inference");

            // Register GitHub Models chat completion
            builder.AddOpenAIChatCompletion(
                modelId: config.ModelName,
                endpoint,
                apiKey: config.ApiKey);

            // Configure execution settings
            OpenAIPromptExecutionSettings githubSettings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                Temperature = config.Temperature,
                MaxTokens = config.MaxTokens
            };
           
            // You can pass these settings when invoking chat completions later
        }
    }
}

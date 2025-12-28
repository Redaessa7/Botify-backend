

using Botify.Domain.Models.AI;
using Botify.Domain.Models.AI.Enums;
using Botify.Infrastructure.AIServices.AIConnectors.Interfaces;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Botify.Infrastructure.AIServices.AIConnectors.Connectors
{
    internal class OpenAIConnectorStrategy : IAIConnectorStrategy
    {
        public string PlatformKey => AIPlatform.OpenAI.ToString();

        public void ConfigureBuilder(IKernelBuilder builder, AIConfiguration config)
        {
            // Register OpenAI chat completion
            builder.AddOpenAIChatCompletion(
                modelId: config.ModelName,
                apiKey: config.ApiKey);

            // Configure execution settings
           OpenAIPromptExecutionSettings openAiSettings = new()
            {
                ToolCallBehavior =  ToolCallBehavior.AutoInvokeKernelFunctions,
                Temperature = config.Temperature,
                MaxTokens = config.MaxTokens
            };

            // You can pass these settings when invoking chat completions later
        }
    }
}

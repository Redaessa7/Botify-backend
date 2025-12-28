using Botify.Domain.Models.AI;
using Botify.Domain.Models.AI.Enums;
using Botify.Infrastructure.AIServices.AIConnectors.Interfaces;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;

namespace Botify.Infrastructure.AIServices.AIConnectors.Connectors
{
    internal class GoogleConnectorStrategy : IAIConnectorStrategy
    {
        public string PlatformKey => AIPlatform.GooglePaLM.ToString();

        public void ConfigureBuilder(IKernelBuilder builder, AIConfiguration config)
        {
            builder.AddGoogleAIGeminiChatCompletion(
                        modelId: config.ModelName,
                        apiKey: config.ApiKey);

            GeminiPromptExecutionSettings geminiSettings = new()
            {
                ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions,
                Temperature = config.Temperature,
                MaxTokens = config.MaxTokens
            };

        }
    }
}
